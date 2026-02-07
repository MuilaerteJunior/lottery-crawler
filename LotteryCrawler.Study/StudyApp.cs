// See https://aka.ms/new-console-template for more information
using LotteryCrawler.Core;
using LotteryCrawler.Core.Crawlers;
using LotteryCrawler.Core.Display;
using LotteryCrawler.Core.Model;
using LotteryCrawler.Core.Strategies.GenerateEngines;
using LotteryCrawler.Core.Strategies.ReadEngines;
using System;
using System.Reflection;

namespace LotteryCrawler.Study
{

    public class StudyApp
    {
        private const int MAX_TRY = Int32.MaxValue;
        private readonly LotteryService<ApostaDTO> _lottery;
        private const string MENU_OPTION_STUDY = "s";
        private const string MENU_OPTION_HELP = "h";
        private const string MENU_OPTION_QUIT = "q";
        private static string[] menuOptions = { MENU_OPTION_STUDY, MENU_OPTION_HELP, MENU_OPTION_QUIT };

        public StudyApp(LotteryService<ApostaDTO> _lott)
        {
            _lottery = _lott;
        }

        private record UserInput(string optionMenu, short gameIndex, bool verbosity = false);
        private static UserInput ProcessArgs(string[] args)
        {
            var optionMenu = args[0] as string;
            if (optionMenu == null || !menuOptions.Contains(optionMenu.ToLower()))
                return new UserInput("?", -1);

            short gameIndex = -1;
            if (args.Length >= 2)
            {
                if (!Int16.TryParse(args[1], out short gameNumber))
                    return new UserInput("?", -1);
            }

            if (args.Length >= 3)
            {
                var  verbosity = args[2] = args[2].ToLower();
                if ( verbosity.Equals("-v") || verbosity.Equals("verbosity") || verbosity.Equals("v") )
                    return new UserInput(optionMenu.ToLower(), gameIndex, true);
            }

            return new UserInput(optionMenu.ToLower(), gameIndex);
        }
        public void Run(string[] args)
        {
            Console.WriteLine("Lottery...");            
            if (args != null && args.Length > 0)
            {
                Console.WriteLine("Starting...");
                var msEngine = new MegaSenaEngine(_lottery);
                int[][] results = msEngine.GetPreviousResults();
                var userArgs = ProcessArgs(args);
                var coreConfig = new CoreConfig();
                if (userArgs != null)
                {                    
                    switch (userArgs.optionMenu)
                    {
                        case MENU_OPTION_HELP: Displayer.ShowStudyAppHelp(); break;
                        case MENU_OPTION_QUIT: break;
                        case MENU_OPTION_STUDY:
                            if (userArgs.gameIndex > 0 && userArgs.gameIndex < results.Length)
                                StudyResults(results.Take(userArgs.gameIndex).ToArray(), userArgs, coreConfig.AvailableEngines, 6);
                            else
                                StudyResults(results, userArgs, coreConfig.AvailableEngines, 6); 
                            break;
                        default:
                            Console.WriteLine($"Choose a valid option between {string.Join("/", menuOptions)}");
                            break;
                    }
                } else
                    Console.WriteLine($"Choose a valid option between {string.Join(" / ", menuOptions)}");
            }
            Console.WriteLine("Finished");
        }
        
        private void StudyResults(int[][] results, UserInput userArgs, Dictionary<string, Tuple<List<IReadEngine>, IGenerateEngine, int?>> generateEngine, short manyNumbers)
        {
            var beginIndex = 0;
            var endIndex = 2;
            Displayer.PrintHeader("Precision study... processing");
            List<Card> outputValues = new List<Card>();
            var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
            _ = Parallel.ForEach(generateEngine, (item) =>
            {
                for (var index = 0; index < results.Length; index++)
                {
                    var pivotIndex = item.Value.Item3;
                    int auxIndex = pivotIndex.HasValue && beginIndex > pivotIndex ? (beginIndex - pivotIndex.Value) + 1 : beginIndex;
                    var subsetResults = results.Skip(auxIndex).Take(endIndex + index);
                    var expectedResult = subsetResults.TakeLast(1).FirstOrDefault();
                    subsetResults = subsetResults.Take(subsetResults.Count() - 1);

                    var generatedBet = GenerateABet(subsetResults, expectedResult, item.Value.Item1, item.Value.Item2, manyNumbers);
                    outputValues.Add(generatedBet);

                    if (userArgs.verbosity && index > 0 && index % 1000 == 0)
                        Console.WriteLine($"{item.Value.Item2.Identification} - Fixed? {item.Value.Item3.HasValue} - {index} already processed...");
                }
            }); 

            Displayer.ShowPreciseResults(outputValues.OrderBy(a => a.EngineName), userArgs.verbosity);
        }

        static Card GenerateABet(IEnumerable<int[]> results, int[] expectedGame, List<IReadEngine> readEngines, IGenerateEngine generateEngine, short manyNumbers)
        {
            var allNumbers = Enumerable.Range(1, 60).Select(a => new BetNumber(a)).ToArray();
            var resultsArray = results.ToArray();
            foreach (var filter in readEngines)
            {
                filter.Read(resultsArray, allNumbers);
            }
            var no = new Card(allNumbers);
            var finalBid = generateEngine.GenerateBet(resultsArray, no, manyNumbers);
            var precision = no.PresenceElements.Where(x => expectedGame.Contains(x.Key)).Select(x => new BetNumber(x.Key, x.Value, 0)).ToArray();

            var finalCardInfo = new Card(allNumbers, precision, finalBid, resultsArray, generateEngine.Identification ?? "unknown");
            return finalCardInfo;
        }
    }
}