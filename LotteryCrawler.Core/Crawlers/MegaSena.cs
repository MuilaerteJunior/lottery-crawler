using HtmlAgilityPack;
using System.Globalization;
using System.Net.Http;

namespace LotteryCrawler.Core.Crawlers
{
    public class MegaSena : LotteryService<ApostaDTO>
    {

        //https://servicebus2.caixa.gov.br/portaldeloterias/api/megasena/2526
        //https://servicebus2.caixa.gov.br/portaldeloterias/_arquivos/loterias/D_megase.zip
        //https://servicebus2.caixa.gov.br/loterias/_arquivos/loterias/D_megase.zip

        //https://servicebus2.caixa.gov.br/portaldeloterias/api/resultados?modalidade=Mega-Sena
        //public MegaSena() : base("http://www1.caixa.gov.br/loterias/_arquivos/loterias/D_megase.zip", "D_MEGA.HTM" , 6 ,8 )

        //https://servicebus2.caixa.gov.br/portaldeloterias/api/megasena/2526
        //https://loterias.caixa.gov.br/Paginas/Mega-Sena.aspx

        public MegaSena(HttpClient httpClient) : base(httpClient)
        {
            
        }

        public override int ObterNumeroResultadoMaisRecente()
        {
            var jogoMaisRecente = ObterJogo(null);
            if (jogoMaisRecente == null)
                throw new Exception("Não foi possível obter o número do resultado mais recente.");

            return jogoMaisRecente.numero;
        }

        public override Queue<Sorteio> ObterTodosJogos(int[] idJogos)
        {
            var filaSorteios = new Queue<Sorteio>();
            Parallel.ForEach(idJogos, (idJogo) =>
            {
                try
                {
                    var jogo = ObterJogo(idJogo);
                    if (jogo != null && jogo.listaDezenas != null && jogo.dataApuracao != null)
                    {
                        filaSorteios.Enqueue(new Sorteio()
                        {
                            Numbers = [.. jogo.listaDezenas.Select(a => Convert.ToInt32(a))],
                            Date = DateTime.ParseExact(jogo.dataApuracao, "dd/MM/yyyy", System.Globalization.CultureInfo.CurrentCulture),
                            Id = jogo.numero,
                        });
                    }
                }
                catch
                {
                    
                }
            });
            return filaSorteios;
        }
    }
}
