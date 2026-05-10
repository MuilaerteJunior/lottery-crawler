namespace LotteryCrawler.Core.Strategies.ReadEngines
{
    public class RecentWindowFrequencyImprovedReadEngine : IReadEngine
    {
        private readonly int _windowSize;

        public RecentWindowFrequencyImprovedReadEngine(int windowSize = 6)
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

            var willNotHappenAgain  = window.TakeLast(2).GroupBy(x => x).Where(x => x.Count() > 1);

            var frequency = window
                .SelectMany(d => d)
                .GroupBy(n => n)
                .ToDictionary(g => g.Key, g => Math.Round((decimal)g.Count() / window.Length, 3));

            foreach (var betNumber in betNumbers)
            {
                if ( willNotHappenAgain.Any(x => x.Key.Contains(betNumber.Number)))
                    betNumber.PositiveProbability = -999;
                else if (frequency.TryGetValue(betNumber.Number, out var prob))
                    betNumber.PositiveProbability -= prob;
            }
        }
    }
}
