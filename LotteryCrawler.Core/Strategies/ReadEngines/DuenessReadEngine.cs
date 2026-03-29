namespace LotteryCrawler.Core.Strategies.ReadEngines
{
    /// <summary>
    /// "Cold number" read engine — boosts the positive probability of numbers
    /// that have been absent from recent draws for the longest time.
    ///
    /// This is the complementary counterpart to ReducePresenceBasedOnPreviousResults:
    ///   - ReducePresence  → penalises numbers that appeared in consecutive recent draws
    ///   - DuenessRead     → rewards numbers that have been overdue (long streak of absence)
    ///
    /// Algorithm:
    ///   For each number, scan history from most recent draw (index 0) backwards
    ///   until the number is found. The draw index at which it was last seen (0-based
    ///   from the most recent end) is the "streak length". If the number was never
    ///   seen, streak = history.Length.
    ///
    ///   boost = Round(streakLength / history.Length, 3)
    ///
    /// A number absent for the entire history gets a boost of 1.0;
    /// a number that appeared in the most recent draw gets a boost of ~0.
    /// </summary>
    public class DuenessReadEngine : IReadEngine
    {
        public void Read(int[][] history, BetNumber[] betNumbers)
        {
            if (history == null || history.Length == 0)
                return;

            foreach (var betNumber in betNumbers)
            {
                var streakLength = history.Length; // assume never seen
                for (var i = history.Length - 1; i >= 0; i--)
                {
                    var draw = history[i];
                    if (draw != null && Array.IndexOf(draw, betNumber.Number) >= 0)
                    {
                        // i is 0-based from oldest; convert to "draws since last seen"
                        // most recent draw is at index history.Length-1
                        streakLength = history.Length - 1 - i;
                        break;
                    }
                }

                betNumber.PositiveProbability =
                    (betNumber.PositiveProbability ?? 0m)
                    + Math.Round((decimal)streakLength / history.Length, 3);
            }
        }
    }
}
