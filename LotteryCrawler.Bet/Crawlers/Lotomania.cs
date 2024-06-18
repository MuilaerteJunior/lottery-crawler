using HtmlAgilityPack;

namespace LotteryCrawler.Bet.Crawlers
{
    public class Lotomania : Lottery
    {
        public Lotomania():base("http://www1.caixa.gov.br/loterias/_arquivos/loterias/D_lotoma.zip", "D_LOTMAN.HTM", 20, 100)
        {
           
        }

        protected override IEnumerable<Sorteio> ReadAndExtractInformation(HtmlDocument htmldoc)
        {
            throw new NotImplementedException();
        }
    }
}
