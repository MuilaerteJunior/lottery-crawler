using LotteryCrawler.Core.Atributes;

namespace LotteryCrawler.Core.Strategies.GenerateEngines
{
    /// <summary>
    /// Deterministic Top-N generator — returns exactly the highest-scored numbers
    /// in descending order of their PresenceElements probability, with zero randomness.
    ///
    /// WHY this improves on WeightedProbabilityEngine:
    ///   WeightedProb is a roulette-wheel: even a number with score 0.001 has a
    ///   non-zero chance of being picked, and a number with score 0.9 might not be
    ///   picked due to random variance. Over a single draw, randomness can cost
    ///   several high-probability numbers from the final bet.
    ///
    ///   TopN eliminates that variance entirely. Given a precise probability signal,
    ///   this is theoretically the maximum-precision generator — it is the Bayes
    ///   optimal classifier under the assumption that the read engines' scores
    ///   correctly rank the winning numbers.
    ///
    /// WHEN to use:
    ///   Pair with strong read pipelines that produce well-calibrated, normalised
    ///   scores (e.g., ExponentialDecay + IntervalRead + MinMaxNormalizer).
    ///   Without normalisation, one dominant engine can drown out all others
    ///   and the ranking is unreliable.
    ///
    /// NOTE:
    ///   This engine is deterministic: given the same history and the same read
    ///   engines, it always produces the same bet. Use WeightedProbabilityConsensusEngine
    ///   for a stochastic alternative that approaches the same ranking with diversity.
    /// </summary>,
    [RankAttribute(1)]
    internal class WeightedProbabilityTopNEngine : IGenerateEngine
    {
        private readonly string _label;

        public WeightedProbabilityTopNEngine(string label = "TopN") => _label = label;

        public string? Identification => _label;

        public BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements)
        {
            return options.PresenceElements
                .OrderByDescending(kvp => kvp.Value)
                .Take(maxNumOfElements)
                .Select(kvp => new BetNumber(kvp.Key, kvp.Value, 0))
                .ToArray();
        }
    }
}
