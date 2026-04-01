namespace LotteryCrawler.Core.Strategies.ReadEngines
{
    public class TestingResults : IReadEngine
    {
        public void Read(int[][] history, BetNumber[] betNumbers)
        {

            var allResults = history.SelectMany(t => t.Select(a => a.ToString("0#")));
            var rightsidelimit = 5;
            var leftsidelimit = 3;
            var rightSide = allResults.GroupBy(x => x[1])
                    .OrderBy(X => X.Count());
                    //.Select(x => $"{x.Key} - Qty: {x.Count()}");//5 - 7 - 3 - 2 - 1 - 4 - 9 - 8 - 0 - 6
            var rightSideAboveLimit = rightSide.Where(x => x.Key >= rightsidelimit).ToDictionary(x => x.Key , x => x.Count());
            var rightSideUnderLimit = rightSide.Where(x => x.Key < rightsidelimit).ToDictionary(x => x.Key, x => x.Count());

            var leftSide = allResults.GroupBy(x => x[0])
                    .OrderBy(X => X.Count());
            //.Select(x => $"{x.Key} - Qty: {x.Count()}");

            var leftSideAboveLimit = leftSide.Where(x => x.Key >= leftsidelimit).ToDictionary(x => x.Key, x => x.Count());
            var leftSideUnderLimit = leftSide.Where(x => x.Key < leftsidelimit).ToDictionary(x => x.Key, x => x.Count());

            foreach (var item in betNumbers)
            {
                var rightSideKey = item.Number.ToString("0#")[1];
                if (rightSideAboveLimit.ContainsKey(rightSideKey))
                {
                    item.PositiveProbability += ((decimal) rightSideAboveLimit[rightSideKey] / allResults.Count());
                }
                else if(rightSideUnderLimit.ContainsKey(rightSideKey))
                {
                    item.PositiveProbability -= ((decimal) rightSideUnderLimit[rightSideKey] / allResults.Count());
                }

                var leftSideKey = item.Number.ToString("0#")[0];
                if (leftSideAboveLimit.ContainsKey(leftSideKey))
                {
                    item.PositiveProbability += ((decimal)leftSideAboveLimit[leftSideKey] / allResults.Count());
                }
                else if (leftSideUnderLimit.ContainsKey(leftSideKey))
                {
                    item.PositiveProbability -= ((decimal)leftSideUnderLimit[leftSideKey] / allResults.Count());
                }
            }
        }
    }



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
