namespace LotteryCrawler.Core.Strategies.GenerateEngines
{
    /// <summary>
    /// Monte Carlo consensus generator — the high-precision evolution of WeightedProbabilityEngine.
    ///
    /// PROBLEM with a single roulette-wheel draw:
    ///   One weighted-random sample introduces high variance. A number with 80% relative
    ///   weight might not appear in a single 15-pick sample just by chance.
    ///
    /// SOLUTION — consensus voting:
    ///   1. Run [simulations] independent weighted-random selections (roulette-wheel),
    ///      each producing maxNumOfElements numbers.
    ///   2. Each selected number receives one vote per simulation it appears in.
    ///   3. After all simulations, return the maxNumOfElements numbers with the most votes.
    ///
    /// WHY this works better:
    ///   - By the Law of Large Numbers, the vote share of number N converges to its
    ///     true selection probability as simulations → ∞.
    ///   - The top-voted numbers ARE the numbers WeightedProb most often picks — this
    ///     is equivalent to taking the empirical mode of the distribution rather than
    ///     a single sample.
    ///   - Unlike TopN, there is still subtle stochasticity (vote ties are broken
    ///     randomly), which prevents the bet from being always identical and avoids
    ///     over-fitting to the exact score order.
    ///
    /// simulations parameter (default 300):
    ///   Higher values → more deterministic, converging toward TopN.
    ///   Lower values  → more diverse, behaves more like WeightedProb.
    ///   300 is a good balance: fast enough for study mode with 2600 games,
    ///   accurate enough to significantly reduce sampling noise.
    /// </summary>
    internal class WeightedProbabilityConsensusEngine : IGenerateEngine
    {
        private readonly int _simulations;
        private readonly string _label;

        public WeightedProbabilityConsensusEngine(int simulations = 300, string label = "Consensus")
        {
            if (simulations <= 0)
                throw new ArgumentOutOfRangeException(nameof(simulations), "Simulations must be > 0.");
            _simulations = simulations;
            _label = label;
        }

        public string? Identification => $"{_label}({_simulations})";

        public BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements)
        {
            var rng = new Random();

            var basePool = options.PresenceElements
                .Select(kvp => (Number: kvp.Key, Weight: kvp.Value))
                .ToList();

            var minWeight = basePool.Min(p => p.Weight);
            var shift = minWeight < 0 ? Math.Abs(minWeight) + 0.001m : 0m;

            // Initialise vote counter for every eligible number
            var votes = new Dictionary<int, int>(basePool.Count);
            foreach (var entry in basePool)
                votes[entry.Number] = 0;

            for (var sim = 0; sim < _simulations; sim++)
            {
                var pool = new List<(int Number, decimal Weight)>(basePool);

                for (var pick = 0; pick < maxNumOfElements && pool.Count > 0; pick++)
                {
                    var totalWeight = pool.Sum(p => p.Weight + shift);
                    var threshold = (decimal)rng.NextDouble() * totalWeight;

                    decimal cumulative = 0m;
                    var selectedIndex = pool.Count - 1; // fallback

                    for (var i = 0; i < pool.Count; i++)
                    {
                        cumulative += pool[i].Weight + shift;
                        if (cumulative >= threshold)
                        {
                            selectedIndex = i;
                            break;
                        }
                    }

                    votes[pool[selectedIndex].Number]++;
                    pool.RemoveAt(selectedIndex);
                }
            }

            // Return the maxNumOfElements numbers with the highest vote counts
            // Ties are broken by the underlying probability score (deterministic within a tie group)
            return votes
                .OrderByDescending(kvp => kvp.Value)
                .ThenByDescending(kvp => options.PresenceElements[kvp.Key])
                .Take(maxNumOfElements)
                .Select(kvp => new BetNumber(kvp.Key, options.PresenceElements[kvp.Key], 0))
                .ToArray();
        }
    }
}
