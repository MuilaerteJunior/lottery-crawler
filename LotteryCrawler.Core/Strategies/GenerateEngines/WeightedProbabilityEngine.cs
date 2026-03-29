namespace LotteryCrawler.Core.Strategies.GenerateEngines
{
    /// <summary>
    /// Roulette-wheel (probability-proportional) selection engine.
    /// Unlike threshold-based engines that treat all eligible numbers equally,
    /// this engine uses the full probability distribution so higher-probability
    /// numbers are proportionally MORE likely to be chosen — not just eligible.
    /// No retry loop is needed because the algorithm always produces exactly
    /// maxNumOfElements picks deterministically.
    /// </summary>
    internal class WeightedProbabilityEngine : IGenerateEngine
    {
        private readonly string _label;

        public WeightedProbabilityEngine(string label = "WeightedProb") => _label = label;

        public string? Identification => _label;

        public BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements)
        {
            var randomGenerator = new Random();

            // Build a mutable pool of (number, weight) pairs with all weights shifted positive
            var pool = options.PresenceElements
                .Select(kvp => (Number: kvp.Key, Weight: kvp.Value))
                .ToList();

            // Shift all weights so minimum is > 0 (required for valid roulette wheel)
            var minWeight = pool.Min(p => p.Weight);
            var shift = minWeight < 0 ? Math.Abs(minWeight) + 0.001m : 0m;

            var result = new List<BetNumber>(maxNumOfElements);
            for (var pick = 0; pick < maxNumOfElements; pick++)
            {
                var totalWeight = pool.Sum(p => p.Weight + shift);
                var threshold = (decimal)randomGenerator.NextDouble() * totalWeight;

                decimal cumulative = 0m;
                int selectedIndex = pool.Count - 1; // fallback to last
                for (var i = 0; i < pool.Count; i++)
                {
                    cumulative += pool[i].Weight + shift;
                    if (cumulative >= threshold)
                    {
                        selectedIndex = i;
                        break;
                    }
                }

                var selected = pool[selectedIndex];
                result.Add(new BetNumber(selected.Number, options.PresenceElements[selected.Number], 0));
                pool.RemoveAt(selectedIndex);
            }

            return result.ToArray();
        }
    }
}
