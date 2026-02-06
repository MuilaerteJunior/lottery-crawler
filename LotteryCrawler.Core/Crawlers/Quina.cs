using HtmlAgilityPack;

namespace LotteryCrawler.Core.Crawlers
{
    public class Quina : Lottery
    {
      
        public Quina() : base("http://www1.caixa.gov.br/loterias/_arquivos/loterias/D_quina.zip", "D_QUINA.HTM", 5, 80)
        {        
        }

        protected override IEnumerable<Sorteio> ReadAndExtractInformation(HtmlDocument htmldoc)
        {
            throw new NotImplementedException();
        }
    }
}
