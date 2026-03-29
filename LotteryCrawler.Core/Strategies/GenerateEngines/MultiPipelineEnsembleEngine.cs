using LotteryCrawler.Core.Strategies.ReadEngines;

namespace LotteryCrawler.Core.Strategies.GenerateEngines
{
    /// <summary>
    /// Six-pipeline committee vote generator — the highest-precision generator in
    /// this solution.
    ///
    /// CORE INSIGHT:
    ///   A number that ranks in the top-K under multiple INDEPENDENT scoring
    ///   perspectives has far stronger evidence supporting its selection than a
    ///   number that dominates under only one perspective.  Additive pipelines
    ///   cannot express this — they collapse all signals into one number where
    ///   scale dominance masks multi-signal agreement.
    ///
    /// HOW IT WORKS:
    ///   Six completely independent read pipelines each score all 60 numbers from
    ///   scratch using the raw history.  Each pipeline nominates its
    ///   top [candidatesPerPipeline] numbers (default 20).  Every nominated number
    ///   receives 1 vote.  The 15 (or maxNumOfElements) numbers with the most
    ///   cross-pipeline votes are returned as the final bet.
    ///
    ///   Ties in vote count are broken by the sum of raw probability scores
    ///   that each tied number received across all pipelines — the pipeline
    ///   that gave the highest score within the tie-breaking group is the secondary
    ///   criterion, providing a smooth, deterministic resolution.
    ///
    /// THE SIX INTERNAL PIPELINES:
    ///
    ///   A — Recency Dominant:
    ///       [ReducePresence, ExponentialDecay(λ=3)]
    ///       Hot numbers weighted by recency-boosted frequency.
    ///
    ///   B — Recency + Overdue Correction:
    ///       [ReducePresence, ExponentialDecay(λ=3), IntervalRead]
    ///       Like A, but numbers that are overdue relative to their own baseline
    ///       interval get an additive boost — counterbalancing pure-recency bias.
    ///
    ///   C — Global Historical Frequency:
    ///       [ReducePresence, WeightBasedOnPresenceV2]
    ///       Stable long-run frequency signal — minimal noise, slow to change.
    ///
    ///   D — Short-Window Trend:
    ///       [ReducePresence, RecentWindowFrequency(50)]
    ///       Only the last 50 draws — captures "current phase" of the draw cycle.
    ///
    ///   E — Associative Context:
    ///       [ReducePresence, CoOccurrence(recentWindowSize=10), SimilarityWeighted(200)]
    ///       Joint-probability signal: numbers that historically co-occurred with
    ///       recent winners, plus numbers from draws structurally similar to the
    ///       most recent outcome.
    ///
    ///   F — Rank Consensus of A+B+C+D (Borda Count):
    ///       RankAggregation(Reduce, ExpDecay, IntervalRead, RecentWindow, WeightV2)
    ///       Numbers that rank consistently high across ALL four frequency signals —
    ///       the most orthogonal, scale-immune meta-signal.
    ///
    /// PARAMETER:
    ///   candidatesPerPipeline (default 20): how many top numbers each pipeline
    ///   nominates.  With 6 pipelines and 15 required picks, a number needs votes
    ///   from at least 2–3 pipelines to be selected — guaranteeing genuine consensus.
    ///   Larger values increase recall (fewer winners miss the cut) at the cost of
    ///   diluting the consensus signal.
    /// </summary>
    internal class MultiPipelineEnsembleEngine : IGenerateEngine
    {
        private readonly int _candidatesPerPipeline;
        private readonly string _label;

        public MultiPipelineEnsembleEngine(int candidatesPerPipeline = 20, string label = "Ensemble")
        {
            if (candidatesPerPipeline <= 0)
                throw new ArgumentOutOfRangeException(nameof(candidatesPerPipeline));
            _candidatesPerPipeline = candidatesPerPipeline;
            _label = label;
        }

        public string? Identification => $"{_label}({_candidatesPerPipeline})";

        // ---------------------------------------------------------------
        // Pipeline definitions (instantiated once at construction — cheap)
        // ---------------------------------------------------------------

        private static readonly IReadEngine[][] Pipelines = BuildPipelines();

        private static IReadEngine[][] BuildPipelines() =>
        [
            // A — Recency Dominant
            [
                new LotteryCrawler.Core.Strategies.ReadEngines.ReducePresenceBasedOnPreviousResults(),
                new LotteryCrawler.Core.Strategies.ReadEngines.ExponentialDecayReadEngine(3.0)
            ],
            // B — Recency + Overdue Correction
            [
                new LotteryCrawler.Core.Strategies.ReadEngines.ReducePresenceBasedOnPreviousResults(),
                new LotteryCrawler.Core.Strategies.ReadEngines.ExponentialDecayReadEngine(3.0),
                new LotteryCrawler.Core.Strategies.ReadEngines.IntervalReadEngine()
            ],
            // C — Global Historical Frequency
            [
                new LotteryCrawler.Core.Strategies.ReadEngines.ReducePresenceBasedOnPreviousResults(),
                new LotteryCrawler.Core.Strategies.ReadEngines.WeightBasedOnPresenceV2()
            ],
            // D — Short-Window Trend
            [
                new LotteryCrawler.Core.Strategies.ReadEngines.ReducePresenceBasedOnPreviousResults(),
                new LotteryCrawler.Core.Strategies.ReadEngines.RecentWindowFrequencyReadEngine(50)
            ],
            // E — Associative Context (co-occurrence + structural similarity)
            [
                new LotteryCrawler.Core.Strategies.ReadEngines.ReducePresenceBasedOnPreviousResults(),
                new LotteryCrawler.Core.Strategies.ReadEngines.CoOccurrenceReadEngine(10),
                new LotteryCrawler.Core.Strategies.ReadEngines.SimilarityWeightedReadEngine(200)
            ],
            // F — Rank-consensus of A/B/C/D (Borda Count meta-signal)
            [
                new LotteryCrawler.Core.Strategies.ReadEngines.RankAggregationReadEngine(
                    new LotteryCrawler.Core.Strategies.ReadEngines.ReducePresenceBasedOnPreviousResults(),
                    new LotteryCrawler.Core.Strategies.ReadEngines.ExponentialDecayReadEngine(3.0),
                    new LotteryCrawler.Core.Strategies.ReadEngines.IntervalReadEngine(),
                    new LotteryCrawler.Core.Strategies.ReadEngines.RecentWindowFrequencyReadEngine(50),
                    new LotteryCrawler.Core.Strategies.ReadEngines.WeightBasedOnPresenceV2()
                )
            ]
        ];

        // ---------------------------------------------------------------

        public BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements)
        {
            // votes[number] → cross-pipeline vote count
            var votes = new Dictionary<int, int>(options.PresenceElements.Count);
            // scoreSum[number] → sum of raw scores from nominating pipelines (tie-break)
            var scoreSum = new Dictionary<int, decimal>(options.PresenceElements.Count);

            foreach (var key in options.PresenceElements.Keys)
            {
                votes[key] = 0;
                scoreSum[key] = 0m;
            }

            foreach (var pipeline in Pipelines)
            {
                // Fresh isolated copy for this pipeline
                var fresh = options.PresenceElements.Keys
                    .Select(n => new BetNumber(n))
                    .ToArray();

                foreach (var engine in pipeline)
                    engine.Read(history, fresh);

                // Nominate top-K from this pipeline
                var nominees = fresh
                    .OrderByDescending(b => b.PositiveProbability ?? decimal.MinValue)
                    .ThenBy(b => b.Number)
                    .Take(_candidatesPerPipeline);

                foreach (var nominee in nominees)
                {
                    votes[nominee.Number]++;
                    scoreSum[nominee.Number] += nominee.PositiveProbability ?? 0m;
                }
            }

            // Return the maxNumOfElements most-voted numbers
            return votes
                .OrderByDescending(kvp => kvp.Value)
                .ThenByDescending(kvp => scoreSum[kvp.Key])
                .Take(maxNumOfElements)
                .Select(kvp => new BetNumber(kvp.Key, options.PresenceElements[kvp.Key], 0))
                .ToArray();
        }
    }
}
