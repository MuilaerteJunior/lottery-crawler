using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotteryCrawler.Core.Strategies.GenerateEngines
{
    internal class Lottery : IGenerateEngine
    {
        public Lottery()
        {
        }

        public string? Identification => "Lottery";

        public BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements)
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
