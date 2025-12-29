// See https://aka.ms/new-console-template for more information
using LotteryCrawler.Bet.Core;
using LotteryCrawler.Bet.Crawlers;
using LotteryCrawler.Bet.Model;
using LotteryCrawler.Bet.Strategies;
using LotteryCrawler.Util;
using Microsoft.Extensions.Options;

namespace LotteryCrawler.Bet
{
    public class App
    {
        private const int NUMBERS_TO_MATCH = 4;
        private static IList<IReadGames>? _strategiesToApply;
        private readonly ConfigVisualizationOptions _config;
        private readonly LotteryService<ApostaDTO> _lottery;

        public App(IOptions<ConfigVisualizationOptions> config, LotteryService<ApostaDTO> _lott)
        {

            _config = config.Value;
            _lottery = _lott;
        }

        private static IList<IReadGames> SetupStrategies(BetNumber[] allNumbers)
        {
            return new List<IReadGames>{
                 new ReducePresenceBasedOnPreviousResults(allNumbers),
                 new EvaluatePresenceBasedOnDecimals(allNumbers),
            };
        }

        public void Run(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                var config = _config;
                var msEngine = new MegaSenaEngine(_lottery);
                int[][] results = msEngine.GetPreviousResults();
                ILottery lottery = new Lottery();

                var optionMenu = args[0] as string;
                int[]? winningGame = null;
                int pickupAttempt = 1000;
                int maxTry = 10000;
                int numberGamesStudy = 30;
                if (args.Length >= 2)
                {
                    Int32.TryParse(args[1], out numberGamesStudy);
                    results = results.TakeLast(numberGamesStudy).ToArray();
                }

                if (optionMenu != null)
                {
                    switch (optionMenu.ToLower())
                    {
                        case "t":
                            Console.WriteLine("Option: Try to guess a game...");

                            if (args.Length >= 3)
                            {
                                try
                                {
                                    winningGame = args[2].Split(",").Select(a => int.Parse(a)).ToArray();
                                }
                                catch
                                {
                                    Console.WriteLine("error processing the parameter");
                                }
                            }

                            if (winningGame == null)
                            {
                                winningGame = results.Last();
                                results = results.Take(results.Length - 1).ToArray();
                                numberGamesStudy = numberGamesStudy - 1;
                            }

                            TryToMatchAGame(_config, numberGamesStudy, results, winningGame, maxTry, lottery);
                            break;
                        case "g":
                            Console.WriteLine("Option: Generating a bet");
                            if (args.Length >= 3)
                                Int32.TryParse(args[2], out pickupAttempt);

                            GiveMeABet(results, lottery, pickupAttempt, _config.HowManyNumbers);
                            break;
                        case "b":
                            int howManyBets = 1;
                            if (args.Length >= 2)
                                Int32.TryParse(args[1], out howManyBets);

                            Console.WriteLine($"Option: Generating bets in batch mode {howManyBets}");
                            for (int i = 0; i < howManyBets; i++)
                            {
                                GiveMeABet(results, lottery, pickupAttempt, _config.HowManyNumbers);
                            }
                            break;
                        case "h":
                            Console.WriteLine("Help");
                            Console.WriteLine("t - see after how many tries it will discover a winning game.");
                            Console.WriteLine("Parameters:");
                            Console.WriteLine($"Number of games to study: default => {numberGamesStudy}");
                            Console.WriteLine("Winning game: default => last game result");
                            Console.WriteLine("g - Generates a game,");
                            Console.WriteLine("Parameters:");
                            Console.WriteLine($"Number of games to study: default => {numberGamesStudy}");
                            Console.WriteLine($"Attempt to pickup: default => {pickupAttempt}");
                            Console.WriteLine("b - Generates games in batch mode,");
                            Console.WriteLine("Parameters:");
                            Console.WriteLine($"Number of games to generate: default => 1");
                            break;
                        case "q":
                            break;
                        default:
                            Console.WriteLine("Choose a valid option between q/g/t");
                            break;
                    }
                }
            }
            Console.ReadKey();
        }


        static void TryToMatchAGame(ConfigVisualizationOptions config, int numberGamesStudy, int[][] pastResults, int[] winnerBet, int maxTry, ILottery lottery)
        {
            Console.WriteLine("Starting...");
            Console.WriteLine($"Using configuration: {numberGamesStudy} games to study");
            Console.WriteLine($"Winner bet: {string.Join("-", winnerBet)} games to study");
            Console.WriteLine($"trying to guess... it could take a few minutes...");
            try
            {
                var allNumbers = Enumerable.Range(1, 60).Select(a => new BetNumber(a)).ToArray();
                _strategiesToApply = SetupStrategies(allNumbers);
                foreach (var filter in _strategiesToApply)
                {
                    allNumbers = filter.Read(pastResults);
                }

                var availableNumbersOptions = lottery.CreateNumbersAndProbabilities(allNumbers);
                var tryIndex = 0;
                var maxMatch = 0;
                var magicalBet = new Queue<MagicalBet>();
                var presenceCount = Enumerable.Repeat(0, 61).ToArray();
                var matchCount = 0;
                do
                {
                    var availableOptionsCopy = new List<int>(availableNumbersOptions);
                    var finalBid = lottery.GenerateBet(availableOptionsCopy, allNumbers, config.HowManyNumbers);
                    var matchedNumbers = finalBid.Intersect(winnerBet);
                    matchCount = matchedNumbers.Count();

                    if (matchCount > maxMatch)
                        maxMatch = matchCount;

                    if (matchCount >= NUMBERS_TO_MATCH)
                        magicalBet.Enqueue(new MagicalBet(finalBid.Intersect(winnerBet), winnerBet.Except(finalBid), tryIndex));


                    for ( var index =0; index < finalBid.Length; index++)
                    {
                        presenceCount[finalBid[index]]++;
                    }

                    tryIndex++;

                    if (!config.ShowOnlyBetsAbove4Matches)
                        Console.WriteLine("Try {3} - Numbers: {0} - Match: {1} ({2}) - Max Match so far: {4} ", string.Join(" - ", finalBid.OrderBy(a => a)), matchCount, string.Join(",", matchedNumbers), tryIndex, maxMatch);
                    else if (matchCount > 4)
                        Console.WriteLine("Try {3} - Numbers: {0} - Match: {1} ({2}) - Max Match so far: {4} ", string.Join(" - ", finalBid.OrderBy(a => a)), matchCount, string.Join(",", matchedNumbers), tryIndex, maxMatch);
                } while (tryIndex < maxTry && matchCount != winnerBet.Length);

                Console.WriteLine("Finished");

                ShowSummary(config, numberGamesStudy, pastResults, winnerBet, availableNumbersOptions, maxMatch, magicalBet, presenceCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
        }

        static void GiveMeABet(int[][] results, ILottery lottery, int maxTry, int howManyNumbers)
        {
            Console.WriteLine("Generating the bet...");
            try
            {
                var allNumbers = Enumerable.Range(1, 60).Select(a => new BetNumber(a)).ToArray();
                _strategiesToApply = SetupStrategies(allNumbers);
                foreach (var filter in _strategiesToApply)
                {
                    allNumbers = filter.Read(results);
                }

                var availableNumbersOptions = lottery.CreateNumbersAndProbabilities(allNumbers);
                var tryIndex = 0;
                var magicalBet = new Queue<MagicalBet>();
                do
                {
                    var availableOptionsCopy = new List<int>(availableNumbersOptions);
                    var finalBid = lottery.GenerateBet(availableOptionsCopy, allNumbers, howManyNumbers);
                    magicalBet.Enqueue(new MagicalBet(finalBid, tryIndex));
                    tryIndex++;
                } while (tryIndex < maxTry);
                Console.WriteLine($"Good Luck! {magicalBet.Last()}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
        }

        #region Show methods

        private static void ShowSummary(ConfigVisualizationOptions config, int numberGamesStudy, int[][] pastResults, int[] winnerBet, List<int> availableNumbersOptions, int maxMatch, Queue<MagicalBet> magicalBet, int[] presenceCount)
        {
            var date = DateTime.Now.ToString("yyyyMMdd_hhmmss");

            OutputFormatter.PrintLineSeparator();
            foreach (var mb in magicalBet)
            {
                Console.WriteLine(mb);
            }
            OutputFormatter.PrintLineSeparator();

            if (config.ShowStudiedGames)
                ShowStudiedGames(pastResults);

            var tAux = presenceCount.Select((count, number) => new Tuple<int, int>(count, number));
            if (config.ShowTop10MostFrequentOnTries)
                ShowTop10MostFrequentOnTries(availableNumbersOptions, tAux);

            if (config.ShowTop10LessFrequentOnTries)
                ShowTop10LessFrequentOnTries(availableNumbersOptions, tAux);

            if (config.ShowProbabilityOfEachNumber)
                ShowProbabilityOfEachNumber(availableNumbersOptions);

            if (config.ShowProbabilityOfExpectedNumbers)
                ShowProbabilityOfExpectedNumbers(winnerBet, availableNumbersOptions);

            if (config.ShowTop20MostLikely)
                ShowTop20MostLikely(availableNumbersOptions);

            if (maxMatch >= 5)
                FileEngine.SaveBestInputsInAFile(date, numberGamesStudy, pastResults, winnerBet, availableNumbersOptions, magicalBet);
        }

        private static void ShowStudiedGames(int[][] pastResults)
        {
            OutputFormatter.PrintSectionTitle("Studied Games");
            foreach (var item in pastResults)
            {
                Console.WriteLine(String.Join(" - ", item.Select(a => a)));
            }
            OutputFormatter.PrintLineSeparator();
        }

        private static void ShowTop10MostFrequentOnTries(List<int> availableNumbersOptions, IEnumerable<Tuple<int, int>> tAux)
        {
            OutputFormatter.PrintOut("Top 10 Most frequent on tries", tAux.OrderByDescending(a => a.Item1).Take(10).Select(a => new NumberPresence
            {
                Number = a.Item2,
                Quantity = a.Item1,
                PossibiltyOfPresence = availableNumbersOptions.Count(b => b == a.Item2)
            }).ToArray());
        }

        private static void ShowTop10LessFrequentOnTries(List<int> availableNumbersOptions, IEnumerable<Tuple<int, int>> tAux)
        {
            OutputFormatter.PrintOut("Top 10 Less frequent on tries", tAux.OrderBy(a => a.Item1).Take(10).Select(a => new NumberPresence
            {
                Number = a.Item2,
                Quantity = a.Item1,
                PossibiltyOfPresence = availableNumbersOptions.Count(b => b == a.Item2)
            }).ToArray());
        }

        private static void ShowProbabilityOfEachNumber(List<int> availableNumbersOptions)
        {
            OutputFormatter.PrintOut("Probability of each numbers", availableNumbersOptions
                                                    .GroupBy(x => x).Select(x => new { x.Key, Count = x.Count() })
                                                    .Select(a => new NumberPresence
                                                    {
                                                        Number = a.Key,
                                                        PossibiltyOfPresence = a.Count
                                                    }).ToArray());
        }

        private static void ShowProbabilityOfExpectedNumbers(int[] winnerBet, List<int> availableNumbersOptions)
        {
            OutputFormatter.PrintOut("Probability of expected numbers", winnerBet
                                                    .Select(a => new NumberPresence
                                                    {
                                                        Number = a,
                                                        PossibiltyOfPresence = availableNumbersOptions.Count(b => a == b)
                                                    }).ToArray());
        }

        private static void ShowTop20MostLikely(List<int> availableNumbersOptions)
        {
            OutputFormatter.PrintOut("Top 20 most likely numbers", availableNumbersOptions
                                                .GroupBy(x => x).Select(x => new { x.Key, Count = x.Count() })
                                                .OrderByDescending(a => a.Count)
                                                .Select(a => new NumberPresence
                                                {
                                                    Number = a.Key,
                                                    PossibiltyOfPresence = a.Count
                                                }).Take(20).ToArray());
        }
        #endregion
    }
}