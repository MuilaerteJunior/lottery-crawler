using LotteryCrawler.Core.Atributes;

namespace LotteryCrawler.Core.Strategies.GenerateEngines
{
    [Rank(6)]
    internal class Lottery : IGenerateEngine
    {
        private readonly string _label;

        public Lottery(string label)
        {
            _label = label;
        }

        public virtual string? Identification => _label;

        public virtual BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements)
        {
            var randomGenerator = new Random();
            var finalBid = new int[maxNumOfElements];
            var threshold = (options.PresenceElements.Max(x => x.Value) + options.PresenceElements.Min(x => x.Value)) / 2;
            var validNumbers = options.PresenceElements.Where(x => x.Value >= threshold).Select(x => x.Key).ToList();
            if (validNumbers.Count < maxNumOfElements)
                validNumbers = options.PresenceElements.Select(x => x.Key).ToList();

            for (var index = 0; index < maxNumOfElements; index++)
            {
                var randomPosition = randomGenerator.Next(0, validNumbers.Count);
                finalBid[index] = validNumbers[randomPosition];
                validNumbers.RemoveAt(randomPosition);
            }

            return finalBid.Select(x => new BetNumber(x, options.PresenceElements[x],0)).ToArray();
        }
    }
}
