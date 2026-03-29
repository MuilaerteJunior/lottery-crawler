namespace LotteryCrawler.Core.Strategies.ReadEngines
{
    /// <summary>
    /// Borda-Count rank aggregation engine.
    ///
    /// PROBLEM it solves — additive score fusion is scale-corrupted:
    ///   When read engines are chained with additive fusion, the engine with the
    ///   largest output range dominates the final ordering.
    ///   ExponentialDecay outputs cumulative sums of hundreds; IntervalRead outputs [0,1].
    ///   MinMaxNormalizer runs AFTER their sum is formed — it is monotone and cannot
    ///   change the ORDER. The ordering was already corrupted before normalisation.
    ///
    /// HOW it works — per-engine independent ranking:
    ///   For each sub-engine:
    ///     1. Create a FRESH BetNumber[] with all scores zeroed.
    ///     2. Run sub-engine.Read(history, freshCopy) — score in isolation.
    ///     3. Sort freshCopy descending → assign rank 1 (best) … 60 (worst).
    ///   Each number gets one rank per sub-engine.
    ///   Final score = (totalNumbers - avgRank + 1) / totalNumbers  →  [0, 1]
    ///   Overwrites incoming betNumbers.PositiveProbability with the final score.
    ///
    /// WHY this reaches higher precision:
    ///   - Scale is irrelevant: every engine always contributes exactly 1 rank unit
    ///     per slot regardless of output magnitude.
    ///   - Multi-signal consensus is rewarded: a number ranked #3 by ExpDecay AND #4
    ///     by IntervalRead gets avgRank=3.5 → score=0.942.
    ///   - Conflict is penalised: ranked #1 by ExpDecay but #55 by IntervalRead gets
    ///     avgRank=28 → score=0.517.  The disagreement correctly reduces confidence.
    ///   This is the Borda Count algorithm — proven optimal for rank aggregation.
    ///
    /// USAGE:
    ///   Use as the primary (and usually sole) read engine in a pipeline:
    ///     var engines = new List&lt;IReadEngine&gt; { new RankAggregationReadEngine(
    ///         new ReducePresenceBasedOnPreviousResults(),
    ///         new ExponentialDecayReadEngine(),
    ///         new IntervalReadEngine(),
    ///         new SimilarityWeightedReadEngine()) };
    ///   Optionally follow with MinMaxNormalizerReadEngine for [0,1] output.
    /// </summary>
    public class RankAggregationReadEngine : IReadEngine
    {
        private readonly IReadEngine[] _subEngines;

        public RankAggregationReadEngine(params IReadEngine[] subEngines)
        {
            if (subEngines == null || subEngines.Length == 0)
                throw new ArgumentException("At least one sub-engine is required.", nameof(subEngines));
            _subEngines = subEngines;
        }

        public void Read(int[][] history, BetNumber[] betNumbers)
        {
            if (history == null || betNumbers.Length == 0)
                return;

            var count = betNumbers.Length;

            // Accumulate ranks across all sub-engines (lower = better)
            var rankSum = new Dictionary<int, double>(count);
            foreach (var b in betNumbers)
                rankSum[b.Number] = 0.0;

            foreach (var engine in _subEngines)
            {
                // Run engine on a FRESH isolated copy
                var fresh = betNumbers
                    .Select(b => new BetNumber(b.Number))
                    .ToArray();

                engine.Read(history, fresh);

                // Sort descending by the net probability from this engine
                var ranked = fresh
                    .OrderByDescending(b => b.PositiveProbability ?? decimal.MinValue)
                    .ThenBy(b => b.Number) // deterministic tie-breaking
                    .ToArray();

                // Assign 1-based ranks and accumulate
                for (var rank = 1; rank <= ranked.Length; rank++)
                    rankSum[ranked[rank - 1].Number] += rank;
            }

            var engineCount = _subEngines.Length;

            // Final score: invert avg rank → [0, 1] (higher = better)
            foreach (var betNumber in betNumbers)
            {
                var avgRank = rankSum[betNumber.Number] / engineCount;
                var score = (decimal)(count - avgRank + 1) / count;

                // Overwrite backing field, preserving NegativeProbability semantics:
                // getter returns (backingField - NegProbability), so set backingField = score + NegProbability
                betNumber.PositiveProbability = Math.Round(score, 4) + betNumber.NegativeProbability;
            }
        }
    }
}
