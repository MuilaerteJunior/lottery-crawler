namespace LotteryCrawler.Core.Strategies.ReadEngines
{
    /// <summary>
    /// Joint-probability co-occurrence read engine.
    ///
    /// PROBLEM it solves — numbers are scored in isolation:
    ///   All existing read engines assign a probability to each number independently.
    ///   None captures the relationship "number 27 almost always appears in the same
    ///   draw as numbers that recently won."  This is a joint-distribution signal.
    ///
    /// HOW it works — anchor-context co-occurrence:
    ///   1. Take the last [recentWindowSize] draws as the "anchor context".
    ///      Collect all unique "anchor numbers" that appeared in those draws.
    ///   2. Scan the FULL history (excluding the anchor window) looking for draws
    ///      that share numbers with the anchor set.
    ///   3. For each such draw:
    ///        overlap = number of anchor numbers it contains
    ///        weight  = overlap / anchorNumbers.Count              (0..1)
    ///      Accumulate weight for every number in that draw.
    ///   4. Normalise all accumulated scores to [0, 1] (divide by max).
    ///   5. ADD normalised score to PositiveProbability (additive boost).
    ///
    /// WHY this increases precision:
    ///   If the last 3 draws contained numbers {4, 22, 37, 15, 51, 8}, and number 27
    ///   has historically appeared many times in draws that also contained several of
    ///   those anchor numbers, then 27 carries co-occurrence evidence that purely
    ///   frequency-based or recency-based engines miss entirely.
    ///
    ///   This is the "collaborative filtering" principle applied to lottery draws:
    ///   "numbers that travel together, stay together."
    ///
    /// USAGE NOTE:
    ///   This is additive. Follow with MinMaxNormalizerReadEngine when chaining
    ///   with other engines to prevent scale dominance.
    ///
    /// Parameter:
    ///   recentWindowSize (default 10) — how many of the most recent draws to use
    ///   as the anchor context. Smaller = more focused on latest trend.
    ///   Larger = more anchor numbers, more historical draws get partial credit.
    /// </summary>
    public class CoOccurrenceReadEngine : IReadEngine
    {
        private readonly int _recentWindowSize;

        public CoOccurrenceReadEngine(int recentWindowSize = 10)
        {
            if (recentWindowSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(recentWindowSize), "Window size must be > 0.");
            _recentWindowSize = recentWindowSize;
        }

        public void Read(int[][] history, BetNumber[] betNumbers)
        {
            if (history == null || history.Length < 2 || betNumbers.Length == 0)
                return;

            var validDraws = history.Where(d => d != null).ToArray();
            if (validDraws.Length < 2)
                return;

            // Anchor context: last recentWindowSize draws (or all if fewer)
            var anchorDraws = validDraws
                .TakeLast(Math.Min(_recentWindowSize, validDraws.Length - 1))
                .ToArray();

            var anchorNumbers = anchorDraws
                .SelectMany(d => d)
                .Distinct()
                .ToHashSet();

            if (anchorNumbers.Count == 0)
                return;

            // Scan draws BEFORE the anchor window
            var historicalDraws = validDraws
                .SkipLast(anchorDraws.Length)
                .ToArray();

            if (historicalDraws.Length == 0)
                return;

            var coScores = new Dictionary<int, decimal>();

            foreach (var draw in historicalDraws)
            {
                var overlap = draw.Count(n => anchorNumbers.Contains(n));
                if (overlap == 0)
                    continue;

                var weight = (decimal)overlap / anchorNumbers.Count;
                foreach (var num in draw)
                {
                    if (!coScores.ContainsKey(num))
                        coScores[num] = 0m;
                    coScores[num] += weight;
                }
            }

            if (coScores.Count == 0)
                return;

            // Normalise to [0, 1]
            var maxScore = coScores.Values.Max();
            if (maxScore <= 0m)
                return;

            foreach (var betNumber in betNumbers)
            {
                if (coScores.TryGetValue(betNumber.Number, out var raw))
                    betNumber.PositiveProbability =
                        (betNumber.PositiveProbability ?? 0m) + Math.Round(raw / maxScore, 4);
            }
        }
    }
}
