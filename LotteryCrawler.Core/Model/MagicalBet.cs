// See https://aka.ms/new-console-template for more information

namespace LotteryCrawler.Core.Model
{
    public class MagicalBet
    {
        public MagicalBet(int[] finalBid, int tryIndex)
        {
            this.Bet = finalBid.OrderBy(a=> a).ToArray();
            this.TryIdentifier = tryIndex;
        }

        public MagicalBet(BetNumber[] finalBid)
        {
            this.FinalNumbers = finalBid.OrderBy(a => a.Number).ToArray();
        }
        public int[] Bet { get { return FinalNumbers.Select(x => x.Number).ToArray(); } set => bet = value; }

        public BetNumber[] FinalNumbers { get; set; }
        public BetNumber[] Options;
        private int[] bet;

        public int TryIdentifier { get; set; }        

        public override string? ToString()
        {
            if (FinalNumbers != null)
                return string.Join(" - ", FinalNumbers.Select(a => a.Number));

            return string.Empty;
        }

        public string? ToStringPrecision()
        {
            if ( FinalNumbers == null || FinalNumbers.Length <= 0)
                return string.Empty;

            return $"{string.Join(",", FinalNumbers.Select(a =>a.Number))} ({string.Join(";", FinalNumbers.Select(a => Math.Round((decimal) (a.PositiveProbability ?? 0m), 3).ToString("0.##")))})";
        }
    }
}
