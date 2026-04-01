using LotteryCrawler.Core.Atributes;

namespace LotteryCrawler.Core.Strategies.GenerateEngines
{
    [Rank(4)]
    internal class SimpleLottery : IGenerateEngine
    {
        public SimpleLottery(string label)
        {
            _label = label;
        }
        private readonly string _label;

        public virtual string? Identification => _label;
        private short MAX_NUM_OF_REPITIONS = 2;
        private short MAX_TRIES = 100;
        private int bottomLimitAvgDifference = 7;
        private int topLimitAvgDifference = 10;

        private Random _randomGenerator = new Random();
        public virtual BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements)
        {
            var finalBid = new int[maxNumOfElements];
            var numIndex = 0;
            var lastGame = history.LastOrDefault();
            if (lastGame != null)
            {
                var howManyRepeatFromPreviousGame = _randomGenerator.Next(0, 2);
                for (var index = howManyRepeatFromPreviousGame; index > 0; index--)
                {
                    finalBid[numIndex++] = lastGame[_randomGenerator.Next(0, lastGame.Length)];
                }
            }
            if ( numIndex == 0)
                finalBid[numIndex++] = options.PresenceElements.OrderByDescending(x => x.Value).First().Key;

            var availableOptions = options.PresenceElements.Where(x => !finalBid.Contains(x.Key)).Select(x => x.Key).ToList();
            for (var index = numIndex; index < maxNumOfElements; index++)
            {
                var randomPosition = _randomGenerator.Next(0, availableOptions.Count);
                var numCandidate = availableOptions[randomPosition];

                var numCandidateGroup = numCandidate.ToString("0#")[0];
                var grouped = finalBid.GroupBy(x => x.ToString("0#")[0]).ToDictionary(x =>  x.Key, x => x.Count());
                if (!grouped.ContainsKey(numCandidateGroup) || grouped[numCandidateGroup] <= 3)
                {
                    finalBid[index] = numCandidate;
                } else {
                    index--;
                }

                availableOptions.RemoveAt(randomPosition);
            }


            return finalBid.Select(x => new BetNumber(x, options.PresenceElements.ContainsKey(x) ? options.PresenceElements[x] : 0, 0)).ToArray();
        }

        private bool CheckAvgDifferenceLimit(int[] game)
        {
            var differences = game.OrderBy(x => x).Select((elemnt, index) => index == 0 ? elemnt - 0 : elemnt - game[index - 1]);
            var avgDifference = differences.Average();
            return avgDifference >= bottomLimitAvgDifference && avgDifference <= topLimitAvgDifference;
        }
    }
}
