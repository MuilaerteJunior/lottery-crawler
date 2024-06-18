namespace LotteryCrawler
{

    public class BetNumber
    {
        private decimal? positiveProbability;

        public BetNumber(int number)
        {
            this.Number = number;
            this.UnlikelyNumber = new Tuple<int, decimal>[] {
                Tuple.Create(number + 1, 0.75m),
                Tuple.Create(number + 2, 0.5m),
            };
        }

        public BetNumber(int number, decimal negProbability, decimal? positiveProbability)
        {
            this.Number = number;
            this.NegativeProbability = negProbability;
            this.PositiveProbability = positiveProbability;
        }

        public int Number { get; private set; }
        public Tuple<int, decimal>[] UnlikelyNumber { get; private set; }
        public decimal? PositiveProbability { get 
            {
                if ( NegativeProbability == 0)
                    return positiveProbability;
                else 
                    return positiveProbability - NegativeProbability;
            } 
            set => positiveProbability = value; }
        public decimal NegativeProbability { get; set; }
    }

}
