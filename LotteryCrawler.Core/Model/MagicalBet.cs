// See https://aka.ms/new-console-template for more information

namespace LotteryCrawler.Core.Model
{
    public class MagicalBet
    {
        public MagicalBet(int[] finalBid, int tryIndex)
        {
            this.FinalNumbers = finalBid.Select(x => x > 0 ? new BetNumber(x) : null).Where(a => a != null).ToArray()!;
            this.TryIdentifier = tryIndex;
        }

        public MagicalBet(BetNumber[] finalBid)
        {
            this.FinalNumbers = finalBid.OrderBy(a => a.Number).ToArray();
        }
        public int[] Bet { get { return FinalNumbers.Select(x => x.Number).ToArray(); } }

        public BetNumber[] FinalNumbers { get; set; }

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
