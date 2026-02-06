using System;

namespace LotteryCrawler.Core.Strategies.ReadEngines
{
    public class EvaluatePresenceBasedOnDecimals : IReadEngine
    {
        public void Read(int[][] history, BetNumber[] betNumbers)
        {
            if (history == null)
                throw new ArgumentNullException("Invalid argument for numbers");

            var currentNumbers = history[0];
            var finalResult = new short[history[0].Length];//1-10,11-20,21-30,31-40,41-50,51-60

            var bidResultsNumbers = history
                                        .Where(a => a != null)
                                        .SelectMany(x => x)
                                        .GroupBy(x => Math.Ceiling((decimal) x/10) * 10)
                                        .Select(a => new { a.Key, Count = a.Count() })
                                        .ToDictionary(a => a.Key, b => b.Count);

            var total = bidResultsNumbers.Sum(x => x.Value);
            foreach (var number in betNumbers)
            {
                var key = (int) Math.Ceiling((decimal)number.Number / 10) * 10;
                if (bidResultsNumbers.ContainsKey(key))
                    number.PositiveProbability = Math.Round((decimal)bidResultsNumbers[key] / total, 2);
            }
        }
    }
}
