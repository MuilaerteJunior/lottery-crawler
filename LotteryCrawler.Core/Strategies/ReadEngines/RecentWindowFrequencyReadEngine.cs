namespace LotteryCrawler.Core.Strategies.ReadEngines
{
    /// <summary>
    /// Frequency-based read engine scoped to a configurable recent window.
    ///
    /// WeightBasedOnPresenceV2 computes frequency over the entire history.
    /// For games with 2600+ draws, results from decades ago dilute the signal
    /// from current trends. This engine isolates frequency to the last N draws
    /// (default: 50) to capture "recent momentum" without noise from distant history.
    ///
    /// Algorithm:
    ///   window = history.TakeLast(windowSize)   (uses all if history is shorter)
    ///   probability = Round(occurrences / window.Length, 3)
    /// </summary>
    public class RecentWindowFrequencyReadEngine : IReadEngine
    {
        private readonly int _windowSize;

        public RecentWindowFrequencyReadEngine(int windowSize = 50)
        {
            if (windowSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(windowSize), "Window size must be greater than zero.");
            _windowSize = windowSize;
        }

        public void Read(int[][] history, BetNumber[] betNumbers)
        {
            if (history == null || history.Length == 0 || betNumbers.Length == 0)
                return;

            var window = history
                .Where(d => d != null)
                .TakeLast(_windowSize)
                .ToArray();

            if (window.Length == 0)
                return;

            var frequency = window
                .SelectMany(d => d)
                .GroupBy(n => n)
                .ToDictionary(g => g.Key, g => Math.Round((decimal)g.Count() / window.Length, 3));

            foreach (var betNumber in betNumbers)
            {
                if (frequency.TryGetValue(betNumber.Number, out var prob))
                    betNumber.PositiveProbability = prob;
            }
        }
    }
}
