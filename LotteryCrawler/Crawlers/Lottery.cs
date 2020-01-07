using LotteryCrawler.Net;
using System.Collections.Generic;
using System.Linq;

namespace LotteryCrawler.Crawlers
{
    public class Lottery
    {
        protected static List<Sorteio> Sorteios;
        private readonly int _mininumBids;
        private StudyNumbers studyNumbers;
        private List<List<int>> MagicBets;
        private string _timeElapsed;

        public Lottery(int numberOfOptions, int resultNumbersCount)
        {

        }

        protected Lottery(string url , string fileName, int bids, int considerNLastBids, bool predictiveMode = false)
        {
            //var resultGetter = new MegaSenaResults(url, fileName, bids);

            //_mininumBids = bids;
            //Sorteios = resultGetter.GetResults();

            //studyNumbers = new StudyNumbers(Sorteios, considerNLastBids);

            //if ( !predictiveMode ) {                
            //    MagicBets = studyNumbers.GenerateBets();
            //} else {
            //    _timeElapsed = studyNumbers.PredictiveBet(Sorteios.First());
            //}
        }        

        public virtual List<int> Drawn(int betIndex)
        {
            return MagicBets[betIndex];
        }

        public string TimeElapsed() {
            return _timeElapsed;
        }
    }
}


