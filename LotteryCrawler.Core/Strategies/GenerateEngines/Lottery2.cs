namespace LotteryCrawler.Core.Strategies.GenerateEngines
{
    internal class Lottery2 : IGenerateEngine
    {
        public Lottery2()
        {
        }

        public string? Identification => "Lottery2";

        public BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements)
        {
            var randomGenerator = new Random();
            var finalBid = new int[maxNumOfElements];
            var threshold = (options.PresenceElements.Max(x => x.Value) + options.PresenceElements.Min(x => x.Value)) / 2;
            var presenceThreshold = 0.06m;
            var validNumbers = options.PresenceElements.Where(x => x.Value >= threshold).Select(x => x.Key).ToList();
            if (validNumbers.Count < maxNumOfElements)
                validNumbers = options.PresenceElements.Select(x => x.Key).ToList();

            var successfulNumbers = false;
            var maxGenerations = 1000;
            while (successfulNumbers == false && maxGenerations > 0)
            {
                var subsetValidNumbers = new List<int>(validNumbers);
                for (var index = 0; index < maxNumOfElements; index++)
                {
                    var randomPosition = randomGenerator.Next(0, subsetValidNumbers.Count);
                    finalBid[index] = subsetValidNumbers[randomPosition];
                    subsetValidNumbers.RemoveAt(randomPosition);
                }

                if (finalBid.Sum(x => options.PresenceElements[x]) <= presenceThreshold)
                    successfulNumbers = true;

                maxGenerations--;
            }

            return finalBid.Select(x => new BetNumber(x, options.PresenceElements[x], 0)).ToArray();
        }
    }
}
