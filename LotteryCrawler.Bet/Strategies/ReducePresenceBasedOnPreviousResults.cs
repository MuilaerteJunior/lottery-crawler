namespace LotteryCrawler.Bet.Strategies
{
    public class ReducePresenceBasedOnPreviousResults : IReadGames
    {
        private static BetNumber[] _availableOptions;

        public ReducePresenceBasedOnPreviousResults(BetNumber[] availableOptions)
        {
            if (availableOptions == null) throw new ArgumentNullException("availableOptions");

            _availableOptions = availableOptions;
        }

        public static decimal GetSplit(int countNum)
        {
            if (countNum <= 0)
                throw new ArgumentException("countNum <= 0");

            return Math.Round((decimal) 1 / (countNum - 1), 2);
        }

        public BetNumber[] Read(int[][] bidResults)
        {
            if (bidResults == null || bidResults.Length < 2)
                throw new ArgumentNullException("Invalid argument for numbers");

            var divisionToRemove = GetSplit(bidResults.Length);
            decimal negProbability = 0.9m;
            var mostRecentResult = bidResults.Last();
            var index = bidResults.Length - 1;
            while (index > 0)
            {
                var auxNumbers = bidResults[--index];
                var intersectNumbers = mostRecentResult.Intersect(auxNumbers);
                if (intersectNumbers.Any())
                {
                    foreach (var number in intersectNumbers)
                    {
                        _availableOptions[number - 1].NegativeProbability -= negProbability;
                    }
                }
                mostRecentResult = auxNumbers;
                negProbability -= divisionToRemove;
            }
            return _availableOptions;
        }
    }

}
