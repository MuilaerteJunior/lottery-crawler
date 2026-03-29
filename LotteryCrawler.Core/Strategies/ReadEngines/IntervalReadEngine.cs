namespace LotteryCrawler.Core.Strategies.ReadEngines
{
    /// <summary>
    /// Supplementary read engine that boosts numbers whose current absence streak
    /// exceeds their own personal average appearance interval.
    ///
    /// Unlike DuenessReadEngine (raw absence count), this engine is self-calibrated
    /// per number: a rare number that typically appears every 50 draws is only
    /// considered "overdue" after 50 draws, not after 5.
    ///
    /// Algorithm:
    ///   1. Build an appearance index: for each number, collect all draw indices
    ///      where it appeared (0 = oldest, n-1 = most recent).
    ///   2. Compute average interval between consecutive appearances.
    ///   3. Current gap = (n-1) - lastSeenIndex.
    ///   4. overdueScore = max(0, (currentGap - avgInterval) / avgInterval), clamped to [0, 1].
    ///   5. Add overdueScore to PositiveProbability.
    ///
    /// Numbers never seen: overdueScore = 1.0 (maximally overdue by definition).
    /// Numbers seen only once: score proportional to gap since that single appearance.
    /// Numbers not yet overdue: score = 0 (no effect).
    /// </summary>
    public class IntervalReadEngine : IReadEngine
    {
        public void Read(int[][] history, BetNumber[] betNumbers)
        {
            if (history == null || history.Length < 2 || betNumbers.Length == 0)
                return;

            var n = history.Length;

            // Build appearance list per number: 0 = oldest, n-1 = most recent
            var appearances = new Dictionary<int, List<int>>();
            for (var i = 0; i < n; i++)
            {
                if (history[i] == null) continue;
                foreach (var num in history[i])
                {
                    if (!appearances.ContainsKey(num)) appearances[num] = new List<int>();
                    appearances[num].Add(i);
                }
            }

            foreach (var betNumber in betNumbers)
            {
                decimal boost;

                if (!appearances.TryGetValue(betNumber.Number, out var positions) || positions.Count == 0)
                {
                    // Never seen in history — maximally overdue
                    boost = 1.0m;
                }
                else if (positions.Count == 1)
                {
                    // Only one data point: gap from that appearance to now
                    var singleGap = n - 1 - positions[0];
                    boost = Math.Round((decimal)singleGap / n, 3);
                }
                else
                {
                    // Compute average interval between consecutive appearances
                    double sumIntervals = 0;
                    for (var i = 1; i < positions.Count; i++)
                        sumIntervals += positions[i] - positions[i - 1];
                    var avgInterval = sumIntervals / (positions.Count - 1);

                    // Current gap: draws since last appearance
                    var currentGap = n - 1 - positions[positions.Count - 1];
                    var overdueScore = (currentGap - avgInterval) / avgInterval;
                    boost = Math.Round((decimal)Math.Max(0, Math.Min(overdueScore, 1.0)), 3);
                }

                betNumber.PositiveProbability = (betNumber.PositiveProbability ?? 0m) + boost;
            }
        }
    }
}
