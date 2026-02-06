namespace LotteryCrawler.Core.Strategies.ReadEngines
{
    public class WeightBasedOnPresenceV2 : IReadEngine
    {
        public void Read(int[][] history, BetNumber[] betNumbers)
        {
            if (history != null && betNumbers.Length > 0)
            {
                var currentResults = history.SelectMany(x => x)
                                .GroupBy(x => x)
                                .ToDictionary(n => n.Key, n => Math.Round((decimal)n.Count() / history.Length, 3));
                foreach(var item in betNumbers)
                {
                    if (currentResults.ContainsKey(item.Number))
                        item.PositiveProbability = currentResults[item.Number];
                }
            }
        }
    }

}
