namespace LotteryCrawler.Core.Strategies.ReadEngines
{
    public class WeightBasedOnPresence : IReadEngine
    {
        public void Read(int[][] history, BetNumber[] betNumbers)
        {
            var weights = new Dictionary<int, decimal>();
            for (int i = 0; i < history.Length; i++)
            {
                decimal weight = (decimal)(i + 1) / history.Length;
                foreach (int n in history[i])
                {
                    if (!weights.ContainsKey(n)) weights[n] = 0;
                    weights[n] += weight;
                }
            }

            foreach (var item in betNumbers)
            {
                if (weights.ContainsKey(item.Number))
                {
                    item.PositiveProbability = weights[item.Number];
                }
            }
        }
    }

}
