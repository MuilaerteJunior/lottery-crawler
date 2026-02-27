namespace LotteryCrawler
{
    public class BetNumber
    {
        private decimal? positiveProbability;

        public BetNumber(int number)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(number, 0, "number");

            this.Number = number;
        }

        public BetNumber(int number, decimal? positiveProbability, decimal negProbability )
        {
            this.Number = number;
            this.PositiveProbability = positiveProbability;
            this.NegativeProbability = negProbability;
        }

        public int Number { get; private set; }
        public decimal? PositiveProbability { get 
            {
                if ( NegativeProbability != 0)
                    return positiveProbability - NegativeProbability;

                return positiveProbability;
            } 
            set => positiveProbability = value; }
        public decimal NegativeProbability { get; set; }
    }

}
