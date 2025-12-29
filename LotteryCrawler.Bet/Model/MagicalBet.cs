// See https://aka.ms/new-console-template for more information

namespace LotteryCrawler.Bet.Model
{
    public class MagicalBet
    {
        public MagicalBet(IEnumerable<int> intersec, IEnumerable<int> enumerable, int tryId)
        {
            this.Bet = intersec.ToArray();
            this.MissedNumbers = enumerable.ToArray();
            this.TryIdentifier = tryId;
        }

        public MagicalBet(int[] finalBid, int tryIndex)
        {
            this.Bet = finalBid.OrderBy(a=> a).ToArray();
            this.TryIdentifier = tryIndex;
        }

        public int[] Bet { get; set; }
        public int[] MissedNumbers { get; set; }
        public BetNumber[] Options;
        public int TryIdentifier { get; set; }        

        public override string? ToString()
        {
            string final = string.Empty;
            if (MissedNumbers != null)
                final = string.Concat(final,"Missing Numbers: ", string.Join("-", MissedNumbers),".");
            if (Bet != null)
                final = string.Concat(final,"Bet: ", string.Join("-", Bet), ".");
            return final;
        }
    }
}
