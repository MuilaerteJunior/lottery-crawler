using LotteryCrawler.Core.Atributes;

namespace LotteryCrawler.Core.Strategies.GenerateEngines
{
    /// <summary>
    /// Lottery-style uniform-random generator with a configurable percentile threshold.
    ///
    /// The original Lottery engine uses the midpoint of (max + min) / 2 as its threshold,
    /// which keeps roughly the top ~50% of numbers.  This engine replaces that with a
    /// percentile-based cutoff so we can empirically test different pool sizes:
    ///
    ///   percentile = 40  →  top 40% of scores kept  → ~24 numbers in pool
    ///   percentile = 50  →  top 50%                  → ~30 numbers (≈ original Lottery)
    ///   percentile = 60  →  top 60%                  → ~36 numbers
    ///   percentile = 25  →  top 25%                  → ~15 numbers (tight pool)
    ///
    /// Selection from the filtered pool is uniform-random (no weighting), exactly like
    /// the original Lottery engine.  This isolates the effect of pool size on precision.
    /// </summary>
    [Rank(3)]
    internal class AdaptiveLotteryEngine : IGenerateEngine
    {
        private readonly string _label;
        private readonly int _percentile;

        public AdaptiveLotteryEngine(int percentile = 50, string label = "AdaptiveLottery")
        {
            _label = label;
            _percentile = Math.Clamp(percentile, 1, 99);
        }

        public string? Identification => _label;

        public BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements)
        {
            var rng = new Random();

            // Sort all numbers by score descending
            var sorted = options.PresenceElements
                .OrderByDescending(kvp => kvp.Value)
                .ToList();

            // Keep top percentile% of numbers
            int poolSize = Math.Max(maxNumOfElements, (int)Math.Ceiling(sorted.Count * _percentile / 100.0));
            var validNumbers = sorted.Take(poolSize).Select(kvp => kvp.Key).ToList();

            // Uniform random selection without replacement (identical to Lottery)
            var selected = new int[maxNumOfElements];
            for (int i = 0; i < maxNumOfElements; i++)
            {
                int idx = rng.Next(0, validNumbers.Count);
                selected[i] = validNumbers[idx];
                validNumbers.RemoveAt(idx);
            }

            return selected
                .Select(n => new BetNumber(n, options.PresenceElements[n], 0))
                .ToArray();
        }
    }
}
