namespace LotteryCrawler.Core.Crawlers
{
    public class ApostaDTO { 

        /// <summary>
        /// Números já ordenados
        /// </summary>
        public string[]? listaDezenas { get; set; }
        /// <summary>
        /// Data do sorteio
        /// </summary>
        public string? dataApuracao { get; set; }
        /// <summary>
        /// Número do concurso
        /// </summary>
        public int numero { get; set; }
    }
}
