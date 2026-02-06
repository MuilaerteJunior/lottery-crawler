namespace LotteryCrawler.Core.Strategies.ReadEngines
{
    public class ReducePresenceBasedOnPreviousResults : IReadEngine
    {
        private readonly decimal _negProbability = 0.9m;
        public ReducePresenceBasedOnPreviousResults() { }
        public ReducePresenceBasedOnPreviousResults(decimal negProbability)
        {
            this._negProbability = negProbability;
        }

        public static decimal GetSplit(int countNum)
        {
            if (countNum <= 0)
                throw new ArgumentException("countNum <= 0");

            return Math.Round((decimal) 1 / (countNum - 1), 2);
        }

        public void  Read(int[][] history, BetNumber[] betNumbers)
        {
            if (history == null )
                    throw new ArgumentNullException("Invalid argument for numbers");

            if (history == null || history.Length < 2)
                return;

            var divisionToRemove = GetSplit(history.Length);
            var mostRecentResult = history.Last();
            var negProbability = _negProbability;
            var index = history.Length - 1;
            while (index > 0)
            {
                var previousResult = history[--index];
                if (previousResult != null)
                {
                    var intersectNumbers = mostRecentResult.Intersect(previousResult);
                    if (intersectNumbers.Any())
                    {
                        foreach (var number in intersectNumbers)
                        {
                            var betNumber = betNumbers.First(a => a.Number == number);
                            betNumber.NegativeProbability -= negProbability;
                        }
                    }
                    mostRecentResult = previousResult;
                    negProbability -= divisionToRemove;
                }
            }
        }
    }
}
