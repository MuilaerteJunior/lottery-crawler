namespace LotteryCrawler.Core.Strategies.ReadEngines
{
    /// <summary>
    /// Pipeline normalizer — scales all PositiveProbability values into [0, 1]
    /// after the upstream read engines have run.
    ///
    /// PROBLEM it solves:
    ///   When read engines are chained, their outputs live on different scales.
    ///   ExponentialDecayReadEngine produces cumulative sums in the range [1, e^λ×N],
    ///   while WeightBasedOnPresenceV2 produces values near [0, 0.3].
    ///   If both run sequentially and ADD to PositiveProbability, the exponential
    ///   engine completely overwhelms the frequency engine — its signal is lost.
    ///   After normalization, each number's relative rank is preserved but the
    ///   generate engine sees a clean [0, 1] distribution where the contrast
    ///   between high and low probability numbers is maximised.
    ///
    /// HOW to use:
    ///   Always place this as the LAST read engine in the pipeline.
    ///   Example: { ExponentialDecay, IntervalRead, MinMaxNormalizer } → TopN/Consensus
    ///
    /// PositiveProbability setter/getter note:
    ///   The getter returns (backingField - NegativeProbability).
    ///   To make the getter return a target value X, we set:
    ///     betNumber.PositiveProbability = X + betNumber.NegativeProbability
    ///   This correctly preserves NegativeProbability semantics through the normalizer.
    /// </summary>
    public class MinMaxNormalizerReadEngine : IReadEngine
    {
        public void Read(int[][] history, BetNumber[] betNumbers)
        {
            if (betNumbers.Length < 2)
                return;

            // Read the current NET probability values via the getter (already subtracts NegativeProbability)
            var netValues = betNumbers
                .Select(b => b.PositiveProbability ?? 0m)
                .ToArray();

            var min = netValues.Min();
            var max = netValues.Max();
            var range = max - min;

            if (range == 0m)
                return; // All identical — nothing to normalise

            for (var i = 0; i < betNumbers.Length; i++)
            {
                var normalized = (netValues[i] - min) / range; // in [0, 1]

                // Restore backing field so that getter (backingField - NegProbability) returns normalized
                betNumbers[i].PositiveProbability = normalized + betNumbers[i].NegativeProbability;
            }
        }
    }
}
