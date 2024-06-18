using LotteryCrawler.Bet.Crawlers;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LotteryCrawler.Bet.Core
{
    internal class MegaSenaEngine
    {
        public MegaSenaEngine(LotteryService<ApostaDTO> lotteryService)
        {
            _lotteryService = lotteryService;
        }

        private const string RESULTADOS_JOGOS_CACHE_NOME_ARQUIVO = "jogos.txt";
        private static int? jogoMaisRecente = default(int?);
        private static LotteryService<ApostaDTO>? _lotteryService;

        public int[][] GetPreviousResults()
        {
            var previousResults = GetAllLoteryResults().ToArray();
            if (previousResults == null)
                throw new NotSupportedException("Unable to retrieve previous results!");
            
            return previousResults;
        }

        private static int[][] GetAllLoteryResults()
        {
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

                Console.WriteLine($"Games not found: {string.Join(',', allGames.Select((a, i) => new { a, i }).Where(b => b.a == null).Select(d => d.i + 1))}");
                Console.WriteLine("Cache file saved!");
            }

            return allGames;
        }
    }
}
