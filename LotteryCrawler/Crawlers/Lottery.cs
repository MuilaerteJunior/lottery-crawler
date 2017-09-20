using LotteryCrawler.Net;
using LotteryCrawler.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotteryCrawler.Crawlers
{
    public class Lottery
    {
        private int _totalNumbers { get;}      

        protected List<int> MagicNumbers;
        protected static List<Sorteio> Sorteios;

        private readonly int _mininumBids;

        protected Lottery(string url , string fileName, int bids, int totalNumbers)
        {
            var resultGetter = new ResultsGetter(url, fileName, bids);

            _mininumBids = bids;
            _totalNumbers = totalNumbers;

            Sorteios = resultGetter.GetResults();
            PrepareLottery();
        }

        protected void PrepareLottery()
        {
            var probs = new Dictionary<int, double>();

            MagicNumbers = new List<int>();

            for (int i = 1; i <= _totalNumbers  ; i++)
            {
                probs[i] =
                      (Sorteios
                           .Where(x => x.Numbers.Any(z => z == i))
                           .OrderByDescending(x => x.Date)
                           .Take(1)
                           .Select(z => (double)z.Date.Ticks / DateTime.Now.Ticks)
                           .First())
                           *
                       ((double)Sorteios.Count(x => x.Numbers.Any(z => z == i)) / Sorteios.Count());
            }

            var normalizer = probs.Select(x => x.Value).ToArray();

            foreach (var n in probs.OrderBy(z => Guid.NewGuid()))
            {
                for (var k = 0; k < 119 * normalizer.Normalize(probs[n.Key]); k++)
                    MagicNumbers.Add(n.Key);
            }
        }

        public virtual List<int> Drawn(int? bids = 0)
        {
            if (!bids.HasValue) bids = _mininumBids;

            var drawn = new List<int>();
            while (drawn.Count < bids)
            {
                var n = MagicNumbers.OrderBy(x => Guid.NewGuid()).First();

                if (!drawn.Contains(n))
                {
                    drawn.Add(n);
                }
            }

            drawn.Sort();

            return drawn;
        }
    }
}
