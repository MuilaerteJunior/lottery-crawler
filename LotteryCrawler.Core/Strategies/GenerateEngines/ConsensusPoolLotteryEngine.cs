using LotteryCrawler.Core.Strategies.ReadEngines;

namespace LotteryCrawler.Core.Strategies.GenerateEngines
{
    /// <summary>
    /// Multi-pipeline consensus pool generator with Lottery-style uniform-random selection.
    ///
    /// WHY THIS SHOULD BEAT LOTTERY:
    ///   Lottery filters to ~30 numbers above a midpoint threshold, then picks 15
    ///   uniformly at random.  Its 6.26% precision (2x random baseline) comes from
    ///   the pool sometimes containing all 6 winners.
    ///
    ///   This engine improves POOL QUALITY by running 4 independent scoring pipelines.
    ///   Each pipeline keeps its top-K numbers.  Numbers that appear in 2+ pipeline
    ///   pools form the "consensus pool."  A number that independently ranks high
    ///   under multiple diverse signals is more likely a true winner than one ranked
    ///   high under a single signal.
    ///
    ///   Selection from the consensus pool is still uniform-random (like Lottery),
    ///   preserving the diversity that deterministic engines lack.
    ///
    /// PARAMETERS:
    ///   topPerPipeline (default 35): how many numbers each pipeline keeps.
    ///   minConsensus   (default 2):  minimum pipeline count for consensus membership.
    /// </summary>
    internal class ConsensusPoolLotteryEngine : IGenerateEngine
    {
        private readonly string _label;
        private readonly int _topPerPipeline;
        private readonly int _minConsensus;

        public ConsensusPoolLotteryEngine(string label = "ConsensusPool", int topPerPipeline = 35, int minConsensus = 2)
        {
            _label = label;
            _topPerPipeline = topPerPipeline;
            _minConsensus = minConsensus;
        }

        public string? Identification => _label;

        // Four diverse scoring pipelines — each sees history independently
        private static readonly IReadEngine[][] Pipelines =
        [
            // A — Proven best: consecutive-repeat penalty + decade frequency
            [new ReducePresenceBasedOnPreviousResults(), new EvaluatePresenceBasedOnDecimals()],
            // B — Exponential recency weighting
            [new ReducePresenceBasedOnPreviousResults(), new ExponentialDecayReadEngine()],
            // C — Short-window frequency
            [new ReducePresenceBasedOnPreviousResults(), new RecentWindowFrequencyReadEngine(50)],
            // D — Recency + overdue correction
            [new ReducePresenceBasedOnPreviousResults(), new ExponentialDecayReadEngine(), new IntervalReadEngine()],
        ];

        public BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements)
        {
            var rng = new Random();

            // --- Score each pipeline independently ---
            var voteCounts = new Dictionary<int, int>();
            for (int i = 1; i <= 60; i++)
                voteCounts[i] = 0;

            foreach (var pipeline in Pipelines)
            {
                // Fresh BetNumber array for each pipeline
                var nums = Enumerable.Range(1, 60).Select(n => new BetNumber(n)).ToArray();
                foreach (var engine in pipeline)
                    engine.Read(history, nums);

                // Rank by net probability, keep top K
                var topK = nums
                    .OrderByDescending(b => b.PositiveProbability ?? 0m)
                    .Take(_topPerPipeline)
                    .Select(b => b.Number)
                    .ToHashSet();

                foreach (var num in topK)
                    voteCounts[num]++;
            }

            // --- Build consensus pool ---
            var consensusPool = voteCounts
                .Where(kvp => kvp.Value >= _minConsensus)
                .Select(kvp => kvp.Key)
                .ToList();

            // If consensus pool is too small, expand with tier-1 (in at least 1 pipeline)
            if (consensusPool.Count < maxNumOfElements)
            {
                var tier1 = voteCounts
                    .Where(kvp => kvp.Value >= 1 && !consensusPool.Contains(kvp.Key))
                    .OrderByDescending(kvp => kvp.Value)
                    .ThenByDescending(kvp => options.PresenceElements.GetValueOrDefault(kvp.Key, 0m))
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var num in tier1)
                {
                    consensusPool.Add(num);
                    if (consensusPool.Count >= maxNumOfElements)
                        break;
                }
            }

            // --- Lottery-style uniform random selection from pool ---
            var pool = new List<int>(consensusPool);
            var selected = new int[Math.Min(maxNumOfElements, pool.Count)];
            for (int i = 0; i < selected.Length; i++)
            {
                int idx = rng.Next(0, pool.Count);
                selected[i] = pool[idx];
                pool.RemoveAt(idx);
            }

            return selected
                .Select(n => new BetNumber(n, options.PresenceElements.GetValueOrDefault(n, 0m), 0))
                .ToArray();
        }
    }
}
