namespace LotteryCrawler
{
    public class Card
    {
        public Card(BetNumber[] numbers)
        {
            DoThis(numbers);
        }

        public Card(BetNumber[] availableNumbers, BetNumber[] resultGame, BetNumber[] finalGame, int[][] history, string engineName)
        {
            DoThis(availableNumbers);

            this.ResultGame = resultGame;
            this.FinalGame = finalGame;
            this.History = history;
            EngineName = engineName;
        }
        
        public Dictionary<int, decimal> PresenceElements { get; private set; } = new Dictionary<int, decimal>();
        public BetNumber[] ResultGame { get; }
        public BetNumber[] FinalGame { get; }
        public IEnumerable<int> MatchedNumbers{ get => FinalGame.Select(x=> x.Number).Intersect(ResultGame.Select(x => x.Number));  }
        public int[][] History { get; }
        public string EngineName { get; }

        private void DoThis(BetNumber[] numbers)
        {
            foreach (var number in numbers)
            {
                if (this.PresenceElements.ContainsKey(number.Number))
                    throw new ArgumentException($"Duplicate number found: {number.Number}");

                this.PresenceElements.Add(number.Number, number.PositiveProbability.HasValue ? number.PositiveProbability.Value : 0m);
            }
        }
    }
}
