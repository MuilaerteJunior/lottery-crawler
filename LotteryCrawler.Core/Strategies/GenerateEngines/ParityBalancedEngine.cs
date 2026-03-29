namespace LotteryCrawler.Core.Strategies.GenerateEngines
{
    /// <summary>
    /// Generates a bet that mirrors the historical odd/even ratio of winning draws.
    ///
    /// All existing engines can accidentally produce bets with 5 even + 1 odd numbers,
    /// which are statistically rare in Mega-Sena results. This engine enforces the
    /// empirical parity distribution while still using probability-proportional
    /// (roulette-wheel) selection within each parity pool.
    ///
    /// Algorithm:
    ///   1. Analyse history to compute the average count of odd numbers per draw.
    ///   2. Round to the nearest integer → targetOdd; targetEven = total - targetOdd.
    ///   3. Split PresenceElements into odd/even pools.
    ///   4. Clamp targets to available pool sizes and fill any deficit from the larger pool.
    ///   5. Within each pool, use weighted random (roulette-wheel) selection.
    /// </summary>
    internal class ParityBalancedEngine : IGenerateEngine
    {
        private readonly string _label;

        public ParityBalancedEngine(string label = "ParityBalanced") => _label = label;

        public string? Identification => _label;

        public BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements)
        {
            var rng = new Random();

            // Determine the historical odd/even target
            var validHistory = history.Where(d => d != null).ToArray();
            int targetOdd;
            if (validHistory.Length > 0)
            {
                var avgOdd = validHistory.Average(d => (double)d.Count(n => n % 2 != 0));
                targetOdd = (int)Math.Round(avgOdd);
            }
            else
            {
                targetOdd = maxNumOfElements / 2;
            }

            var targetEven = maxNumOfElements - targetOdd;

            // Partition the available numbers by parity
            var oddPool = options.PresenceElements
                .Where(kvp => kvp.Key % 2 != 0)
                .Select(kvp => (Number: kvp.Key, Weight: kvp.Value))
                .ToList();

            var evenPool = options.PresenceElements
                .Where(kvp => kvp.Key % 2 == 0)
                .Select(kvp => (Number: kvp.Key, Weight: kvp.Value))
                .ToList();

            // Clamp targets so we never exceed the available pool
            targetOdd = Math.Min(targetOdd, oddPool.Count);
            targetEven = Math.Min(targetEven, evenPool.Count);

            // If clamping left us short, fill from the larger pool
            var deficit = maxNumOfElements - (targetOdd + targetEven);
            if (deficit > 0)
            {
                if (oddPool.Count - targetOdd >= evenPool.Count - targetEven)
                    targetOdd += deficit;
                else
                    targetEven += deficit;
            }

            var result = new List<BetNumber>(maxNumOfElements);
            result.AddRange(WeightedPick(oddPool, targetOdd, rng, options.PresenceElements));
            result.AddRange(WeightedPick(evenPool, targetEven, rng, options.PresenceElements));

            return result.ToArray();
        }

        private static IEnumerable<BetNumber> WeightedPick(
            List<(int Number, decimal Weight)> pool,
            int count,
            Random rng,
            Dictionary<int, decimal> presenceElements)
        {
            var result = new List<BetNumber>(count);
            pool = new List<(int, decimal)>(pool); // local copy so we can remove

            var minWeight = pool.Count > 0 ? pool.Min(p => p.Weight) : 0m;
            var shift = minWeight < 0 ? Math.Abs(minWeight) + 0.001m : 0m;

            for (var i = 0; i < count && pool.Count > 0; i++)
            {
                var totalWeight = pool.Sum(p => p.Weight + shift);
                var threshold = (decimal)rng.NextDouble() * totalWeight;
                decimal cumulative = 0m;
                var selectedIndex = pool.Count - 1;

                for (var j = 0; j < pool.Count; j++)
                {
                    cumulative += pool[j].Weight + shift;
                    if (cumulative >= threshold) { selectedIndex = j; break; }
                }

                var selected = pool[selectedIndex];
                result.Add(new BetNumber(selected.Number, presenceElements[selected.Number], 0));
                pool.RemoveAt(selectedIndex);
            }

            return result;
        }
    }
}
