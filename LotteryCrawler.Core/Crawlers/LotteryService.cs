using System.Text.Json;

namespace LotteryCrawler.Core.Crawlers
{
    public abstract class LotteryService<T>(HttpClient httpClient)
            where T: ApostaDTO, new()
    {
        private static IEnumerable<Sorteio>? _sorteios;
        protected readonly int _bids;
        private readonly HttpClient _httpClient = httpClient;

        protected static IEnumerable<Sorteio> Sorteios { get => _sorteios != null ? _sorteios : Enumerable.Empty<Sorteio>(); set => _sorteios = value; }

        public abstract Queue<Sorteio> ObterTodosJogos(int[] idJogos);
        public abstract int ObterNumeroResultadoMaisRecente();

        protected T? ObterJogo(int? numeroJogo)
        {
            if (_httpClient == null) throw new NullReferenceException("_httpClient");
            if (_httpClient.BaseAddress == null) throw new NullReferenceException("_httpClient.BaseAddress");

            var jogo = new T();
            var currentUrl = string.Concat(_httpClient.BaseAddress.AbsoluteUri, (numeroJogo.HasValue ? $"/{numeroJogo}" : ""));
            Console.WriteLine($"CurrentUrl {currentUrl}");
            var response = _httpClient.GetAsync(currentUrl);
            Task.WaitAll(response);
                
            using (var result = response.Result){
                Console.WriteLine($"Status {result.StatusCode}");
                result.EnsureSuccessStatusCode();
                using var reader = result.Content.ReadAsStringAsync();
                jogo = JsonSerializer.Deserialize<T>(reader.Result);
            }
            return jogo;
        }

    }
}


