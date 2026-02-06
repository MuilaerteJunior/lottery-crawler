using HtmlAgilityPack;
using LotteryCrawler.Net;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LotteryCrawler.Core.Crawlers
{
    public abstract class LotteryService<T>// : ILotteryService 
            where T: ApostaDTO, new()
    {
        private const int _bufferSize = 1024 * 16;
        protected static IEnumerable<Sorteio> Sorteios;
        private string _apiEndpoint;
        protected readonly int _bids;
        private string _dataFileName;
        private HttpClient _httpClient;

        public abstract Queue<Sorteio> ObterTodosJogos(int[] idJogos);
        public abstract int ObterNumeroResultadoMaisRecente();

        public LotteryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        private string ReadFile(string filePath)
        {
            var finalResult = "";
            using (var stream = new StreamReader(filePath))
            {
                finalResult = stream.ReadToEnd();
            }
            return finalResult;
        }

        protected T ObterJogo(int? numeroJogo)
        {
            var jogo = new T();
            var currentUrl = string.Concat(_httpClient.BaseAddress.AbsoluteUri, (numeroJogo.HasValue ? $"/{numeroJogo}" : ""));
            Console.WriteLine($"CurrentUrl {currentUrl}");
            var response = _httpClient.GetAsync(currentUrl);
            Task.WaitAll(response);
                
            using (var result = response.Result){
                Console.WriteLine($"Status {result.StatusCode}");
                result.EnsureSuccessStatusCode();
                using (var reader = result.Content.ReadAsStringAsync())
                {
                    jogo = JsonSerializer.Deserialize<T>(reader.Result);
                }
            }
            return jogo;
        }


        private string ExtractFileToMemory(string filePath)
        {
            string finalResult = string.Empty;
            using (ZipArchive archive = new ZipArchive(File.OpenRead(filePath)))
            {
                var entry = archive.Entries.FirstOrDefault(x => x.Name.Equals(_dataFileName, StringComparison.CurrentCultureIgnoreCase));
                if (entry != null)
                {
                    using (var stream = entry.Open())
                    {
                        finalResult = new StreamReader(stream).ReadToEnd();
                    }
                }
            }
            return finalResult;
        }
    }
}


