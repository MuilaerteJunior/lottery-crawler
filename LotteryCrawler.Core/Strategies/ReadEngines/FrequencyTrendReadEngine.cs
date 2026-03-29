namespace LotteryCrawler.Core.Strategies.ReadEngines
{
    /// <summary>
    /// Compares each number's frequency in a short recent window vs. a longer window
    /// to detect trending numbers.
    ///
    /// Numbers "trending up" (appearing more often recently than historically) receive
    /// a small additive boost.  Numbers "trending down" receive a small penalty.
    ///
    /// DESIGN: Boosts are intentionally TINY (±0.01–0.02) so they gently nudge the
    /// pool boundary without overwhelming the base scoring from ReducePresence +
    /// EvaluateDecimals.  This avoids the trap that killed all previous engines: a
    /// strong signal that concentrates selection on a narrow set.
    ///
    /// PARAMETERS:
    ///   shortWindow (default 20): recent draws for "current trend"
    ///   longWindow  (default 100): deeper history for "baseline frequency"
    ///   maxBoost    (default 0.02): cap on additive boost per number
    /// </summary>
    public class FrequencyTrendReadEngine : IReadEngine
    {
        private readonly int _shortWindow;
        private readonly int _longWindow;
        private readonly decimal _maxBoost;

        public FrequencyTrendReadEngine(int shortWindow = 20, int longWindow = 100, decimal maxBoost = 0.02m)
        {
            _shortWindow = shortWindow;
            _longWindow = longWindow;
            _maxBoost = maxBoost;
        }

        public void Read(int[][] history, BetNumber[] betNumbers)
        {
            if (history == null || history.Length < 2)
                return;

            var validHistory = history.Where(d => d != null).ToArray();
            if (validHistory.Length < 2)
                return;

            int shortLen = Math.Min(_shortWindow, validHistory.Length);
            int longLen = Math.Min(_longWindow, validHistory.Length);

            // Count occurrences in each window
            var shortCounts = new Dictionary<int, int>();
            var longCounts = new Dictionary<int, int>();

            for (int i = validHistory.Length - shortLen; i < validHistory.Length; i++)
                foreach (int n in validHistory[i])
                    shortCounts[n] = shortCounts.GetValueOrDefault(n, 0) + 1;

            for (int i = validHistory.Length - longLen; i < validHistory.Length; i++)
                foreach (int n in validHistory[i])
                    longCounts[n] = longCounts.GetValueOrDefault(n, 0) + 1;

            foreach (var bet in betNumbers)
            {
                int num = bet.Number;
                decimal shortFreq = shortCounts.GetValueOrDefault(num, 0) / (decimal)shortLen;
                decimal longFreq = longCounts.GetValueOrDefault(num, 0) / (decimal)longLen;

                if (longFreq == 0)
                    continue;

                decimal ratio = shortFreq / longFreq;

                if (ratio > 1m)
                {
                    // Trending up — small boost, capped
                    decimal boost = Math.Min(_maxBoost * (ratio - 1m), _maxBoost);
                    bet.PositiveProbability = (bet.PositiveProbability ?? 0m) + boost;
                }
                else if (ratio < 1m)
                {
                    // Trending down — smaller penalty
                    decimal penalty = Math.Min(_maxBoost * 0.5m * (1m - ratio), _maxBoost * 0.5m);
                    bet.PositiveProbability = (bet.PositiveProbability ?? 0m) - penalty;
                }
            }
        }
    }
}
