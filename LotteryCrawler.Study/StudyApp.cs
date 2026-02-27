// See https://aka.ms/new-console-template for more information
using LotteryCrawler.Core;
using LotteryCrawler.Core.Crawlers;
using LotteryCrawler.Core.Display;
using LotteryCrawler.Core.Strategies.GenerateEngines;
using LotteryCrawler.Core.Strategies.ReadEngines;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LotteryCrawler.Study.Tests")]
namespace LotteryCrawler.Study
{
    public class StudyApp
    {
        private const int MAX_TRY = Int32.MaxValue;
        private readonly LotteryService<ApostaDTO> _lottery;
        private const string MENU_OPTION_STUDY = "s";
        private const string MENU_OPTION_HELP = "h";
        private const string MENU_OPTION_RESULTS= "r";
        private const string MENU_OPTION_QUIT = "q";
        private static string[] menuOptions = { MENU_OPTION_STUDY, MENU_OPTION_HELP, MENU_OPTION_RESULTS, MENU_OPTION_QUIT };

        public StudyApp(LotteryService<ApostaDTO> _lott)
        {
            _lottery = _lott;
        }

        

        internal  class UserInput
        {
            public string optionMenu;
            public short gameIndex;
            public bool verbosity;
            public short howManyNumbers;

            public UserInput(string optionMenu, short gameIndex = -1, bool verbosity = false, short howManyNumbers = 6)
            {
                this.optionMenu = optionMenu;
                this.gameIndex = gameIndex;
                this.verbosity = verbosity;
                this.howManyNumbers = howManyNumbers;
            }
        }
        internal static UserInput ProcessArgs(string[] args)
        {
            var userInput = new UserInput("?", -1);
            for (int argIndex = 0; argIndex < args.Length; argIndex++)
            {
                string arg = args[argIndex];
                if (arg.Equals(MENU_OPTION_STUDY))
                {
                    userInput.optionMenu = MENU_OPTION_STUDY;
                    if (argIndex < args.Length - 1) {
                        if (Int16.TryParse(args[++argIndex], out short howManyNumbersLocal))
                        {
                            userInput.howManyNumbers = howManyNumbersLocal;
                        }
                    }

                } 
                else if (arg.Equals(MENU_OPTION_HELP) || arg.Equals(MENU_OPTION_RESULTS) || arg.Equals(MENU_OPTION_QUIT))
                {
                    userInput = new UserInput(arg);
                    break;
                }
                else if (args[argIndex].Equals("-v") || args[argIndex].Equals("verbosity") || args[argIndex].Equals("v"))
                {
                    userInput.verbosity = true;
                }
            }

            return userInput;
        }
        public void Run(string[] args)
        {
            Console.WriteLine("Lottery!");            
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
                        case MENU_OPTION_RESULTS: Displayer.ShowStudyAppResults(results); break;
                        case MENU_OPTION_QUIT: break;
                        case MENU_OPTION_STUDY:
                            if (userArgs.gameIndex > 0 && userArgs.gameIndex < results.Length)
                                StudyResults(results.Take(userArgs.gameIndex).ToArray(), userArgs, coreConfig.AvailableEngines, userArgs.howManyNumbers);
                            else
                                StudyResults(results, userArgs, coreConfig.AvailableEngines, userArgs.howManyNumbers); 
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

                    // Fix: Only call GenerateABet if expectedResult is not null
                    if (expectedResult != null)
                    {
                        var generatedBet = GenerateABet(subsetResults, expectedResult, item.Value.Item1, item.Value.Item2, manyNumbers, index + 1);
                        outputValues.Add(generatedBet);
                    }

                    if (userArgs.verbosity && index > 0 && index % 1000 == 0)
                        Console.WriteLine($"{item.Value.Item2.Identification} - Fixed? {item.Value.Item3.HasValue} - {index} already processed...");
                }
            }); 

            Displayer.ShowPreciseResults(outputValues.OrderBy(a => a.EngineName).ThenBy(a=> a.GameIndex), userArgs.verbosity);
        }

        static Card GenerateABet(IEnumerable<int[]> results, int[] expectedGame, 
                                List<IReadEngine> readEngines, 
                                IGenerateEngine generateEngine, 
                                short manyNumbers,
                                int gameIndex)
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

            var finalCardInfo = new Card(allNumbers, precision, finalBid, resultsArray, generateEngine.Identification ?? "unknown", gameIndex);
            return finalCardInfo;
        }
    }
}