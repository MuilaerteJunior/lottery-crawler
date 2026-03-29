namespace LotteryCrawler.Core.Strategies.GenerateEngines
{
    /// <summary>
    /// Combines probability-proportional (roulette-wheel) selection with a
    /// dynamically computed sum constraint derived from the actual historical
    /// distribution of winning draws.
    ///
    /// Lottery3 uses a hardcoded sum range (66–332), which is the theoretical
    /// maximum span for 6 numbers from 1–60. Real winning sums are clustered
    /// in a much narrower band. This engine computes that band from history at
    /// runtime (10th–90th percentile by default) and rejects candidates whose
    /// sum falls outside it.
    ///
    /// Because weighted random selection already biases toward high-probability
    /// numbers (which tend to have moderate values), the rejection rate is low
    /// in practice, typically needing far fewer than the 500-attempt safety cap.
    ///
    /// Parameters:
    ///   lowerPercentile  — lower bound of the target sum range (default 0.10)
    ///   upperPercentile  — upper bound of the target sum range (default 0.90)
    /// </summary>
    internal class SumConstrainedWeightedEngine : IGenerateEngine
    {
        private readonly double _lowerPercentile;
        private readonly double _upperPercentile;
        private readonly string _label;

        public SumConstrainedWeightedEngine(double lowerPercentile = 0.10, double upperPercentile = 0.90, string label = "SumConstrained")
        {
            _lowerPercentile = lowerPercentile;
            _upperPercentile = upperPercentile;
            _label = label;
        }

        public string? Identification => _label;

        public BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements)
        {
            var rng = new Random();

            // Compute the target sum range from history draws with the same size
            var sums = history
                .Where(d => d != null && d.Length == maxNumOfElements)
                .Select(d => d.Sum())
                .OrderBy(s => s)
                .ToArray();

            int minSum, maxSum;
            if (sums.Length >= 2)
            {
                var lowerIdx = (int)Math.Floor(_lowerPercentile * (sums.Length - 1));
                var upperIdx = (int)Math.Ceiling(_upperPercentile * (sums.Length - 1));
                minSum = sums[lowerIdx];
                maxSum = sums[upperIdx];
            }
            else
            {
                // Theoretical fallback: 1+2+…+N  to  (61-N)+…+60
                minSum = Enumerable.Range(1, maxNumOfElements).Sum();
                maxSum = Enumerable.Range(61 - maxNumOfElements, maxNumOfElements).Sum();
            }

            var basePool = options.PresenceElements
                .Select(kvp => (Number: kvp.Key, Weight: kvp.Value))
                .ToList();

            var minWeight = basePool.Min(p => p.Weight);
            var shift = minWeight < 0 ? Math.Abs(minWeight) + 0.001m : 0m;

            BetNumber[]? result = null;
            var maxAttempts = 500;

            while (result == null && maxAttempts-- > 0)
            {
                var pool = new List<(int Number, decimal Weight)>(basePool);
                var picked = new List<int>(maxNumOfElements);

                for (var pick = 0; pick < maxNumOfElements && pool.Count > 0; pick++)
                {
                    var totalWeight = pool.Sum(p => p.Weight + shift);
                    var threshold = (decimal)rng.NextDouble() * totalWeight;
                    decimal cumulative = 0m;
                    var selectedIndex = pool.Count - 1;

                    for (var i = 0; i < pool.Count; i++)
                    {
                        cumulative += pool[i].Weight + shift;
                        if (cumulative >= threshold) { selectedIndex = i; break; }
                    }

                    picked.Add(pool[selectedIndex].Number);
                    pool.RemoveAt(selectedIndex);
                }

                var sum = picked.Sum();
                if (sum >= minSum && sum <= maxSum)
                    result = picked.Select(n => new BetNumber(n, options.PresenceElements[n], 0)).ToArray();
            }

            // Safety fallback: return the last generated bet even if out of range
            if (result == null)
            {
                var pool = new List<(int Number, decimal Weight)>(basePool);
                var picked = new List<int>(maxNumOfElements);

                for (var pick = 0; pick < maxNumOfElements && pool.Count > 0; pick++)
                {
                    var totalWeight = pool.Sum(p => p.Weight + shift);
                    var threshold = (decimal)rng.NextDouble() * totalWeight;
                    decimal cumulative = 0m;
                    var selectedIndex = pool.Count - 1;

                    for (var i = 0; i < pool.Count; i++)
                    {
                        cumulative += pool[i].Weight + shift;
                        if (cumulative >= threshold) { selectedIndex = i; break; }
                    }

                    picked.Add(pool[selectedIndex].Number);
                    pool.RemoveAt(selectedIndex);
                }

                result = picked.Select(n => new BetNumber(n, options.PresenceElements[n], 0)).ToArray();
            }

            return result;
        }
    }
}
