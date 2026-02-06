namespace LotteryCrawler
{
    public class BetNumber
    {
        private decimal? positiveProbability;

        public BetNumber(int number)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(number, 0, "number");

            this.Number = number;
            //this.UnlikelyNumber = new Tuple<int, decimal>[] {
            //    Tuple.Create(number + 1, 0.75m),
            //    Tuple.Create(number + 2, 0.5m),
            //};
        }

        //public BetNumber(int number, decimal negProbability, decimal? positiveProbability)
        public BetNumber(int number, decimal? positiveProbability, decimal negProbability )
        {
            this.Number = number;
            this.PositiveProbability = positiveProbability;
            this.NegativeProbability = negProbability;
        }

        public int Number { get; private set; }
        //public Tuple<int, decimal>[] UnlikelyNumber { get; private set; }
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
