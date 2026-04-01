using LotteryCrawler.Core.Atributes;

namespace LotteryCrawler.Core.Strategies.GenerateEngines
{
    [Rank(4)]
    internal class NewLottery : IGenerateEngine
    {
        
        
        public NewLottery(string label)
        {
            _label = label;
        }

        public virtual string? Identification => _label;
        private short MAX_NUM_OF_REPITIONS = 2;
        private short MAX_TRIES = 100;
        private int bottomLimitAvgDifference = 7;
        private int topLimitAvgDifference = 10;
        private readonly string _label;

        public virtual BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements)
        {
            var randomGenerator = new Random();
            var threshold = (options.PresenceElements.Max(x => x.Value) + options.PresenceElements.Min(x => x.Value)) / 2;

            var lastGame = new int[maxNumOfElements];
            if (history != null && history.Length > 0)
                lastGame = history.Last();

            var finalBid = new int[maxNumOfElements];
            int numTries = 0;
            do
            {
                finalBid = new int[maxNumOfElements];
                var limitOfRepetitions = MAX_NUM_OF_REPITIONS;
                var validNumbers = options.PresenceElements.Where(x => x.Value >= threshold).Select(x => x.Key).ToList();
                if (validNumbers.Count < maxNumOfElements)
                    validNumbers = options.PresenceElements.Select(x => x.Key).ToList();

                for (var index = 0; index < maxNumOfElements; index++)
                {
                    var randomPosition = randomGenerator.Next(0, validNumbers.Count);
                    var number = validNumbers[randomPosition];
                    if ((lastGame.Contains(number) && limitOfRepetitions > 0) || !lastGame.Contains(number))
                    {
                        finalBid[index] = number;
                        validNumbers.RemoveAt(randomPosition);
                    }

                    if (lastGame.Contains(number) && limitOfRepetitions > 0)
                    {
                        limitOfRepetitions--;
                        index--;
                    }

                    if (validNumbers.Count == 0)
                    {
                        validNumbers = options.PresenceElements.Where(x => !finalBid.Contains(x.Key)).Select(x => x.Key).ToList();
                    }
                }
            } while (!CheckAvgDifferenceLimit(finalBid));// && numTries++ < MAX_TRIES);

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
