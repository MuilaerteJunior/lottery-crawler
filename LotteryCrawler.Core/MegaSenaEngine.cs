using LotteryCrawler.Core.Crawlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
[assembly: InternalsVisibleTo("LotteryCrawler.App")]
[assembly: InternalsVisibleTo("LotteryCrawler.Study")]

namespace LotteryCrawler.Core
{
    internal class MegaSenaEngine : BaseLotteryEngine
    {
        public MegaSenaEngine(LotteryService<ApostaDTO> lotteryService) : base(lotteryService)
        {
        }

        protected override string RESULTADOS_JOGOS_CACHE_NOME_ARQUIVO => "jogos.txt";
        public override int[]? GetOptions()
        {
            return Enumerable.Range(1, 60).ToArray();
        }


        public override Func<Card, bool> VerifyMatch1()
        {
            return a => a.MatchedNumbers.Count() == 6;
        }
        public override Func<Card, bool> VerifyMatch2()
        {
            return a => a.MatchedNumbers.Count() == 5;
        }
        public override Func<Card, bool> VerifyMatch3()
        {
            return a => a.MatchedNumbers.Count() == 4;
        }
        public override Func<Card, bool> ValuableResult()
        {
            return a => a.MatchedNumbers.Count() > 3;
        }


    }
}


