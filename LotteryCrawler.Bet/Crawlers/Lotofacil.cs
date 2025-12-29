using HtmlAgilityPack;

namespace LotteryCrawler.Bet.Crawlers
{
    public class Lotofacil : Lottery<object>
    {
        public Lotofacil():base("http://www1.caixa.gov.br/loterias/_arquivos/loterias/D_lotfac.zip", "D_LOTFAC.HTM", 15, 25)
        {
           
        }

        protected override IEnumerable<Sorteio> ReadAndExtractInformation(HtmlDocument htmldoc)
        {
            throw new NotImplementedException();
        }
    }
}
