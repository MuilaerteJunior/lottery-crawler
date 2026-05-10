namespace LotteryCrawler.Core.Strategies.GenerateEngines
{
    internal class TopPriorityEngine : IGenerateEngine
    {
        private readonly string _label;
        public TopPriorityEngine(string label)
        {
            _label = label;
        }
        public virtual string? Identification => _label;
        public virtual BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements)
        {
            var finalBid = new int[maxNumOfElements];
            return  options.PresenceElements.OrderByDescending(x => x.Value).Select(x => new BetNumber(x.Key)).Take(maxNumOfElements).ToArray();
        }
    }
}
