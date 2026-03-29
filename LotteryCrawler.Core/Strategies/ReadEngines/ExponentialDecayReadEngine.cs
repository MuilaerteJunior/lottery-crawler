namespace LotteryCrawler.Core.Strategies.ReadEngines
{
    /// <summary>
    /// Primary read engine that weights draws using an exponential decay curve,
    /// assigning far greater influence to recent draws than to older ones.
    ///
    /// WeightBasedOnPresence (linear) gives the most recent draw N times more
    /// weight than the oldest. ExponentialDecayReadEngine amplifies that contrast:
    /// with lambda=3 the most recent draw gets e^3 ≈ 20× more weight than the
    /// oldest, making "current form" the dominant signal while still considering
    /// historical context.
    ///
    /// Algorithm:
    ///   For each draw at index i (0 = oldest, n-1 = most recent):
    ///     weight_i = Exp(lambda × i / (n - 1))
    ///   Accumulate weight_i for every number in that draw.
    ///   Assign accumulated weight directly as PositiveProbability (overwrite).
    ///
    /// Lambda controls intensity: higher ⇒ more recency-biased (default 3.0).
    /// </summary>
    public class ExponentialDecayReadEngine : IReadEngine
    {
        private readonly double _lambda;

        public ExponentialDecayReadEngine(double lambda = 3.0)
        {
            if (lambda <= 0)
                throw new ArgumentOutOfRangeException(nameof(lambda), "Lambda must be greater than zero.");
            _lambda = lambda;
        }

        public void Read(int[][] history, BetNumber[] betNumbers)
        {
            if (history == null || history.Length == 0 || betNumbers.Length == 0)
                return;

            var n = history.Length;
            var weights = new Dictionary<int, decimal>();

            for (var i = 0; i < n; i++)
            {
                if (history[i] == null) continue;
                // i=0 is oldest draw, i=n-1 is most recent draw
                var weight = (decimal)Math.Exp(_lambda * (double)i / Math.Max(n - 1, 1));
                foreach (var num in history[i])
                {
                    if (!weights.ContainsKey(num)) weights[num] = 0m;
                    weights[num] += weight;
                }
            }

            foreach (var betNumber in betNumbers)
            {
                if (weights.TryGetValue(betNumber.Number, out var w))
                    betNumber.PositiveProbability = Math.Round(w, 3);
            }
        }
    }
}
