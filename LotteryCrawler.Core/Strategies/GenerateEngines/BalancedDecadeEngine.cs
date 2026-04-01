using LotteryCrawler.Core.Atributes;

namespace LotteryCrawler.Core.Strategies.GenerateEngines
{
    /// <summary>
    /// Ensures the generated bet is structurally distributed across numeric decades
    /// (1-10, 11-20, 21-30, 31-40, 41-50, 51-60).
    /// Historical Mega-Sena results show winning numbers are almost always spread
    /// across the full range. This engine enforces that distribution while still
    /// respecting the probability ranking within each decade group.
    ///
    /// For the standard 6-number / 60-range case, each decade gets exactly 1 slot.
    /// For other configurations the slots are allocated proportionally to the aggregate
    /// probability weight of each decade.
    /// </summary>    
    [Rank(6)]
    internal class BalancedDecadeEngine : IGenerateEngine
    {
        private readonly string _label;

        public BalancedDecadeEngine(string label = "BalancedDecade") => _label = label;

        public string? Identification => _label;

        public BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements)
        {
            var randomGenerator = new Random();

            // Group numbers by decade: key = ceiling(number/10)*10  (e.g. 1-10 → 10, 11-20 → 20)
            var decades = options.PresenceElements
                .GroupBy(kvp => (int)(Math.Ceiling(kvp.Key / 10.0m) * 10))
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Key = g.Key,
                    Numbers = g.OrderByDescending(kvp => kvp.Value).ToList(),
                    AggregateWeight = g.Sum(kvp => kvp.Value)
                })
                .ToList();

            // Distribute maxNumOfElements slots proportionally to aggregate weight
            var slots = AllocateSlots(decades.Select(d => d.AggregateWeight).ToList(), maxNumOfElements);

            var result = new List<BetNumber>(maxNumOfElements);

            for (var d = 0; d < decades.Count; d++)
            {
                var decade = decades[d];
                var count = slots[d];
                if (count == 0) continue;

                // Weighted random selection within this decade
                var pool = decade.Numbers
                    .Select(kvp => (Number: kvp.Key, Weight: kvp.Value))
                    .ToList();

                var minWeight = pool.Min(p => p.Weight);
                var shift = minWeight < 0 ? Math.Abs(minWeight) + 0.001m : 0m;

                for (var pick = 0; pick < count && pool.Count > 0; pick++)
                {
                    var totalWeight = pool.Sum(p => p.Weight + shift);
                    var threshold = (decimal)randomGenerator.NextDouble() * totalWeight;

                    decimal cumulative = 0m;
                    int selectedIndex = pool.Count - 1;
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
            }

            return result.ToArray();
        }

        /// <summary>
        /// Floor-based proportional allocation with remainder distributed to
        /// the decades with the highest fractional parts.
        /// </summary>
        private static int[] AllocateSlots(List<decimal> weights, int total)
        {
            var count = weights.Count;
            var totalWeight = weights.Sum();
            var slots = new int[count];

            if (totalWeight <= 0)
            {
                // Equal distribution fallback
                for (var i = 0; i < total; i++)
                    slots[i % count]++;
                return slots;
            }

            var fractions = new decimal[count];
            var remaining = total;
            for (var i = 0; i < count; i++)
            {
                var exact = weights[i] / totalWeight * total;
                slots[i] = (int)Math.Floor(exact);
                fractions[i] = exact - slots[i];
                remaining -= slots[i];
            }

            // Distribute remaining slots to decades with highest fractional parts
            var ranked = Enumerable.Range(0, count)
                .OrderByDescending(i => fractions[i])
                .Take(remaining);
            foreach (var i in ranked)
                slots[i]++;

            return slots;
        }
    }
}
