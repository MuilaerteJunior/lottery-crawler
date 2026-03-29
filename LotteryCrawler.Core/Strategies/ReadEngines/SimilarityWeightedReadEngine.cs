namespace LotteryCrawler.Core.Strategies.ReadEngines
{
    /// <summary>
    /// Associative context engine — boosts numbers that appeared in historical draws
    /// that were structurally similar to the most recent winning draw.
    ///
    /// IDEA:
    ///   If the most recent draw was [5, 12, 23, 34, 45, 56], and a historical draw
    ///   also contained 12 and 34, those two draws share "context". Numbers that
    ///   frequently appeared alongside several of the most-recent winning numbers
    ///   receive a higher boost — even if they are not overdue or globally frequent.
    ///
    /// ALGORITHM:
    ///   1. Take the most recent draw (lastDraw) from history.
    ///   2. Scan the previous [windowSize] draws.
    ///   3. For each draw, compute overlap with lastDraw.
    ///      similarity = overlapCount / lastDraw.Length   (0..1)
    ///   4. For every number in that draw, accumulate (similarity) as a weighted vote.
    ///   5. Normalise by the sum of all accumulated votes and add to PositiveProbability.
    ///
    /// This is additive — it should be followed by MinMaxNormalizerReadEngine to
    /// prevent it from dominating other signals.
    ///
    /// Window parameter (default 200):
    ///   Larger windows capture longer-range associations; smaller windows focus on
    ///   recent structural context only.
    /// </summary>
    public class SimilarityWeightedReadEngine : IReadEngine
    {
        private readonly int _windowSize;

        public SimilarityWeightedReadEngine(int windowSize = 200)
        {
            if (windowSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(windowSize), "Window size must be > 0.");
            _windowSize = windowSize;
        }

        public void Read(int[][] history, BetNumber[] betNumbers)
        {
            if (history == null || history.Length < 2 || betNumbers.Length == 0)
                return;

            // Most recent winning draw supplies the "reference pattern"
            var lastDraw = history.Last(d => d != null);

            // Scan the window of draws BEFORE the most recent (exclude lastDraw itself)
            var window = history
                .Where(d => d != null)
                .SkipLast(1)          // exclude the reference draw to avoid self-similarity bias
                .TakeLast(_windowSize)
                .ToArray();

            if (window.Length == 0)
                return;

            var accumulated = new Dictionary<int, decimal>();

            foreach (var draw in window)
            {
                var overlap = draw.Intersect(lastDraw).Count();
                if (overlap == 0)
                    continue;

                // Similarity in (0, 1]
                var similarity = (decimal)overlap / lastDraw.Length;

                foreach (var num in draw)
                {
                    if (!accumulated.ContainsKey(num))
                        accumulated[num] = 0m;
                    accumulated[num] += similarity;
                }
            }

            if (accumulated.Count == 0)
                return;

            var total = accumulated.Values.Sum();

            foreach (var betNumber in betNumbers)
            {
                if (accumulated.TryGetValue(betNumber.Number, out var w))
                    betNumber.PositiveProbability =
                        (betNumber.PositiveProbability ?? 0m) + Math.Round(w / total, 4);
            }
        }
    }
}
