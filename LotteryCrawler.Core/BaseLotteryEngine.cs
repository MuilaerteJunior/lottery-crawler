using LotteryCrawler.Core.Crawlers;
using System.Text.Json;

namespace LotteryCrawler.Core
{
    public interface ILotteryEngine
    {
        int[]? GetOptions();
        int[][] GetPreviousResults();

        Func<Card, bool> VerifyMatch1();
        Func<Card, bool> VerifyMatch2();
        Func<Card, bool> VerifyMatch3();
        Func<Card, bool> ValuableResult();

    }
    internal abstract class BaseLotteryEngine : ILotteryEngine
    {
        public BaseLotteryEngine(LotteryService<ApostaDTO> lotteryService)
        {
            _lotteryService = lotteryService;
        }
        public abstract int[]? GetOptions();
        public abstract Func<Card, bool> VerifyMatch1();
        public abstract Func<Card, bool> VerifyMatch2();
        public abstract Func<Card, bool> VerifyMatch3();
        public abstract Func<Card, bool> ValuableResult();
        protected abstract string RESULTADOS_JOGOS_CACHE_NOME_ARQUIVO { get; }
        private static int? jogoMaisRecente = default;
        private static LotteryService<ApostaDTO>? _lotteryService;

        public int[][] GetPreviousResults()
        {
            var previousResults = GetAllLoteryResults().ToArray();
            if (previousResults == null)
                throw new NotSupportedException("Unable to retrieve previous results!");

            return previousResults;
        }

        private int[][] GetAllLoteryResults()
        {
            if (_lotteryService == null)
                throw new NullReferenceException("_lotteryService");
            if (!jogoMaisRecente.HasValue)
                jogoMaisRecente = _lotteryService.ObterNumeroResultadoMaisRecente();

            var missingGames = Enumerable.Range(1, jogoMaisRecente.Value);
            var allGames = new int[missingGames.Count()][];

            if (File.Exists(RESULTADOS_JOGOS_CACHE_NOME_ARQUIVO))
            {
                var fileContent = File.ReadAllText(RESULTADOS_JOGOS_CACHE_NOME_ARQUIVO);
                var fileContentAsMatrix = JsonSerializer.Deserialize<int[][]>(fileContent);
                if (fileContentAsMatrix != null)
                {
                    for (int index = 0; index < fileContentAsMatrix.Length; index++)
                    {
                        allGames[index] = fileContentAsMatrix[index];
                    }

                    var existingInCacheGames = allGames.Select((a, i) => new { a, i }).Where(b => b.a != null).Select(d => d.i + 1);
                    missingGames = missingGames.Except(existingInCacheGames);
                }
            }

            if (missingGames.Any())
            {
                var jogos = _lotteryService.ObterTodosJogos(missingGames.ToArray());
                var notfoundFiles = new Queue<int>();
                foreach (var jogo in jogos)
                {
                    try
                    {
                        if (jogo != null)
                            allGames[jogo.Id - 1] = jogo.Numbers.ToArray();
                    }
                    catch (Exception)
                    {
                    }
                }

                using (var fileJogos = new StreamWriter(RESULTADOS_JOGOS_CACHE_NOME_ARQUIVO, false))
                {
                    fileJogos.WriteLine(JsonSerializer.Serialize(allGames));
                }

                var stillNotFound = allGames.Select((a, i) => new { a, i }).Where(b => b.a == null).Select(d => d.i + 1);
                if ( stillNotFound.Any())
                    Console.WriteLine($"Games not found: {string.Join(',', stillNotFound)}");
                else
                    Console.WriteLine("All games downloaded.");

                Console.WriteLine("Cached file saved!");
            }

            return allGames;
        }
    }
}


