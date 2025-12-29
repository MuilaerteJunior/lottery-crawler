namespace LotteryCrawler.Bet.Strategies
{
    public class EvaluatePresenceBasedOnDecimals : IReadGames
    {
        public EvaluatePresenceBasedOnDecimals(BetNumber[] allOPtions)
        {
            _allOptions = allOPtions;
        }

        private BetNumber[] _allOptions;

        public BetNumber[] Read(int[][] bidResults)
        {
            if (bidResults == null || bidResults.Length < 2)
                throw new ArgumentNullException("Invalid argument for numbers");

            var currentNumbers = bidResults[0];
            var finalResult = new short[6];//1-10,11-20,21-30,31-40,41-50,51-60

            var bidResultsNumbers = bidResults
                                        .SelectMany(x => x)
                                        .GroupBy(x => Math.Ceiling((decimal) x/10) * 10)
                                        .Select(a => new { a.Key, Count = a.Count() })
                                        .ToArray();

            var total = bidResultsNumbers.Sum(x => x.Count);

            for (var index = 0; index < _allOptions.Length; index++)
            {
                int aux = (int) Math.Ceiling((decimal)(index + 1) / 10) - 1;
                var decimalIndex = bidResultsNumbers[aux];
                _allOptions[index].PositiveProbability = Math.Round((decimal )decimalIndex.Count / total, 2);
            }
            
            return _allOptions;
        }
    }

}
