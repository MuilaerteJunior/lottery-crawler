using HtmlAgilityPack;
using System.Globalization;
using System.Net.Http;

namespace LotteryCrawler.Core.Crawlers
{

    public class ApostaDTO { 

        /// <summary>
        /// Números já ordenados
        /// </summary>
        public string[] listaDezenas { get; set; }
        /// <summary>
        /// Data do sorteio
        /// </summary>
        public string dataApuracao { get; set; }
        /// <summary>
        /// Número do concurso
        /// </summary>
        public int numero { get; set; }
    }


    public class MegaSena : LotteryService<ApostaDTO>
    {
        private string _url;
        private string _dataFileName;
        //private int _drawnCount;

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
                    filaSorteios.Enqueue(new Sorteio()
                    {
                        Numbers = jogo.listaDezenas.Select(a => Convert.ToInt32(a)).ToList(),
                        Date = DateTime.ParseExact(jogo.dataApuracao, "dd/MM/yyyy", System.Globalization.CultureInfo.CurrentCulture),
                        Id = jogo.numero,
                    });
                }
                catch
                {
                    var aa = "";
                }
            });
            return filaSorteios;
        }
    }
}
