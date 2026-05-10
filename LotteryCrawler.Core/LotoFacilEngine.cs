using LotteryCrawler.Core.Crawlers;

namespace LotteryCrawler.Core
{
    internal class LotoFacilEngine : BaseLotteryEngine {

        public LotoFacilEngine(LotteryService<ApostaDTO> lotteryService) : base(lotteryService)
        {
        }

        protected override string RESULTADOS_JOGOS_CACHE_NOME_ARQUIVO =>  "jogos_lotofacil.txt";

        public override int[]? GetOptions()
        {
            return Enumerable.Range(1, 25).ToArray();
        }



        public override Func<Card, bool> VerifyMatch1()
        {
            return a => a.MatchedNumbers.Count() == 15;
        }
        public override Func<Card, bool> VerifyMatch2()
        {
            return a => a.MatchedNumbers.Count() == 14;
        }
        public override Func<Card, bool> VerifyMatch3()
        {
            return a => a.MatchedNumbers.Count() == 13;
        }
        public override Func<Card, bool> ValuableResult()
        {
            return a => a.MatchedNumbers.Count() >= 11;
        }
    }
}


