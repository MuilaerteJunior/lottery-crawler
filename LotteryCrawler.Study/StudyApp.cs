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
        //private readonly LotteryService<ApostaDTO> _lottery;
        private const string MENU_OPTION_STUDY = "s"; 
        private const string MENU_OPTION_EVALUATE = "e";
        private const string MENU_OPTION_HELP = "h";
        private const string MENU_OPTION_RESULTS= "r";
        private const string MENU_OPTION_STRESS= "x";
        private const string MENU_OPTION_QUIT = "q";
        private static string[] menuOptions = { MENU_OPTION_STUDY, MENU_OPTION_HELP, MENU_OPTION_RESULTS, MENU_OPTION_QUIT };
        
        public StudyApp()//LotteryService<ApostaDTO> _lott)
        {
            //_lottery = _lott;
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
                if (arg.Equals(MENU_OPTION_STUDY) || arg.Equals(MENU_OPTION_STRESS))
                {
                    userInput.optionMenu = arg;
                    if (argIndex < args.Length - 1) {
                        if (Int16.TryParse(args[++argIndex], out short howManyNumbersLocal))
                        {
                            userInput.howManyNumbers = howManyNumbersLocal;
                        }
                    }
                }
                else if (arg.Equals(MENU_OPTION_HELP) 
                        || arg.Equals(MENU_OPTION_RESULTS) 
                        || arg.Equals(MENU_OPTION_QUIT) 
                        || arg.Equals(MENU_OPTION_EVALUATE))
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

        private Tuple<int[], int[][]> Download(ILotteryEngine lotteryEngine)
        {
            var numberOptions = lotteryEngine.GetOptions() ?? Array.Empty<int>();
            int[][] results = lotteryEngine.GetPreviousResults();
            return Tuple.Create(numberOptions, results);
        }


        public void Run(string[] args)
        {
            Console.WriteLine("Lottery!");            
            if (args != null && args.Length > 0)
            {
                Console.WriteLine("Starting...");
                var engine = new MegaSenaEngine(new MegaSena(new HttpClient()));
                var megaSena = Download(engine);
                var lotoFacil = Download(new LotoFacilEngine(new LotoFacil(new HttpClient())));
                var results = megaSena.Item2;
                var numberOptions = megaSena.Item1;

                var userArgs = ProcessArgs(args);
                var coreConfig = new CoreConfig();
                if (userArgs != null)
                {                    
                    switch (userArgs.optionMenu)
                    {
                        case MENU_OPTION_HELP: Displayer.ShowStudyAppHelp(); break;
                        case MENU_OPTION_RESULTS: Displayer.ShowStudyAppResults(results); break;
                        case MENU_OPTION_EVALUATE: EvaluateReadEngines(coreConfig.AvailableEngines.SelectMany(x => x.Value.Item1).Distinct().ToList(), results, numberOptions); break;//s 15 v
                        //case MENU_OPTION_STRESS: Stress(results, userArgs, coreConfig.AvailableEngines, userArgs.howManyNumbers, numberOptions, engine); break;
                        case MENU_OPTION_STRESS: Stress(results, userArgs, coreConfig.AvailableEngines, 15, numberOptions, engine); break;
                        case MENU_OPTION_QUIT: break;
                        case MENU_OPTION_STUDY:
                            if (userArgs.gameIndex > 0 && userArgs.gameIndex < results.Length)
                                StudyResults(results.Take(userArgs.gameIndex).ToArray(), userArgs, coreConfig.AvailableEngines, userArgs.howManyNumbers, numberOptions);
                            else
                                StudyResults(results, userArgs, coreConfig.AvailableEngines, userArgs.howManyNumbers, numberOptions); 
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

        private void Stress(int[][] results, UserInput userArgs, Dictionary<string, Tuple<List<IReadEngine>, IGenerateEngine, int?>> generateEngine, short manyNumbers, IEnumerable<int> numberOptions, ILotteryEngine engine)
        {
            var rankingInfo = new Ranking();
            var limit = 30;
            for (var index = 0; index < limit; index++)
            {
                var beginIndex = 0;
                var endIndex = 2;
                Displayer.PrintHeader($"Stressing... Iteration {index + 1}/{limit}  ");
                List<Card> outputValues = new List<Card>();
                Process(results, userArgs, generateEngine, manyNumbers, numberOptions, beginIndex, endIndex, outputValues);
                rankingInfo.SummarizePerformance(outputValues, engine);
                rankingInfo.Output();
            }
            Displayer.PrintHeader("FINAL RESULTS");
            rankingInfo.Output();
        }

        private void EvaluateReadEngines(List<IReadEngine> readEngines, int[][] results, IEnumerable<int> numberOptions)
        {
            int beginIndex = 0, endIndex = results.Length;
            var evaluationResults 
                = new Dictionary<string, List<(bool firstLevelUnder, bool secondLevelUnder, bool thirdLevelUnder)>>();
            _ = Parallel.ForEach(readEngines, (readEngine) =>
            {
                var list = new List<(bool firstLevelUnder, bool secondLevelUnder, bool thirdLevelUnder)>();
                for (var index = 0; index < results.Length - 2; index++)
                {
                    var allNumbers = numberOptions.Select(a => new BetNumber(a)).ToArray();
                    var subsetResults = results.Take(index + 1);
                    var result = results.ToArray()[index + 2];
                    var resultsArray = subsetResults.ToArray();
                    readEngine.Read(resultsArray, allNumbers);

                    var betNumbers = allNumbers.OrderByDescending(a => a.PositiveProbability).Select(x => x.Number);
                    var firstLevelUnder = result.Except(betNumbers.Take(50)).Count() == 0;
                    var secondLevelUnder = result.Except(betNumbers.Take(45)).Count() == 0;
                    var thirdLevelUnder = result.Except(betNumbers.Take(10)).Count() == 0;

                    list.Add((firstLevelUnder, secondLevelUnder, thirdLevelUnder));
                }
                evaluationResults.Add(readEngine.GetType().Name + "_" + Guid.NewGuid()  , list);
            });

            int rankingPosition = 0;
            foreach (var item in evaluationResults.OrderByDescending(x=> x.Value.Count(a => a.thirdLevelUnder))
                .ThenByDescending(x => x.Value.Count(a => a.secondLevelUnder)))
            {
                var total = item.Value.Count;
                var firstLevelUnder = item.Value.Count(a => a.firstLevelUnder);
                var secondLevelUnder = item.Value.Count(a => a.secondLevelUnder );
                var thirdLevelUnder = item.Value.Count(a => a.thirdLevelUnder);
                Console.WriteLine($"{Environment.NewLine}Ranking {++rankingPosition}"+
                                   $"{Environment.NewLine}Engine: {item.Key} "+
                                   $"{Environment.NewLine}Under 50: {firstLevelUnder} ({((decimal)firstLevelUnder / total) * 100:0.00}%)"+
                                   $"{Environment.NewLine}Under 45: {secondLevelUnder} ({((decimal)secondLevelUnder / total) * 100:0.00}%)"+ 
                                   $"{Environment.NewLine}Under 10: {thirdLevelUnder} ({((decimal)thirdLevelUnder / total) * 100:0.00}%)");
            }   
        }
        
        private void StudyResults(int[][] results, UserInput userArgs, Dictionary<string, Tuple<List<IReadEngine>, IGenerateEngine, int?>> generateEngine, short manyNumbers, IEnumerable<int> numberOptions)
        {
            var beginIndex = 0;
            var endIndex = 2;
            Displayer.PrintHeader("Precision study... processing");
            List<Card> outputValues = new List<Card>();
            Process(results, userArgs, generateEngine, manyNumbers, numberOptions, beginIndex, endIndex, outputValues);

            Displayer.ShowPreciseResults(outputValues.OrderBy(a => a.EngineName).ThenBy(a => a.GameIndex), userArgs.verbosity);
        }

        private static void Process(int[][] results, UserInput userArgs, Dictionary<string, Tuple<List<IReadEngine>, IGenerateEngine, int?>> generateEngine, short manyNumbers, IEnumerable<int> numberOptions, int beginIndex, int endIndex, List<Card> outputValues)
        {
            var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
            _ = Parallel.ForEach(generateEngine, (item) =>
            {
                for (var index = 0; index < results.Length; index++)
                {
                    try
                    {
                        var pivotIndex = item.Value.Item3;
                        int auxIndex = pivotIndex.HasValue && beginIndex > pivotIndex ? (beginIndex - pivotIndex.Value) + 1 : beginIndex;

                        var subsetResults = results.AsSpan(auxIndex, endIndex + index > results.Length?  results.Length : endIndex + index);
                        //Skip(auxIndex).Take(endIndex + index);
                        var expectedResult = subsetResults.Slice(subsetResults.Length - 1)[0];
                        //.TakeLast(1).FirstOrDefault();
                        subsetResults = subsetResults.Slice(0, subsetResults .Length - 1);
                        //.Take(subsetResults.Count() - 1);

                        // Fix: Only call GenerateABet if expectedResult is not null
                        if (expectedResult != null)
                        {
                            var generatedBet = GenerateABet(subsetResults, expectedResult, item.Value.Item1, item.Value.Item2, manyNumbers, index + 1, numberOptions, item.Key);
                            outputValues.Add(generatedBet);
                        }

                        if (userArgs.verbosity && index > 0 && index % 1000 == 0)
                            Console.WriteLine($"{item.Value.Item2.Identification} - Fixed? {item.Value.Item3.HasValue} - {index} already processed...");
                    }
                    catch (Exception Ex)
                    {
                        Console.WriteLine(Ex);
                    }
                }
            });
        }

        static Card GenerateABet(Span<int[]> results, int[] expectedGame, 
                                List<IReadEngine> readEngines, 
                                IGenerateEngine generateEngine, 
                                short manyNumbers,
                                int gameIndex,
                                IEnumerable<int> numberOptions,
                                string keyIdentifier )
        {
            var allNumbers = numberOptions.Select(a => new BetNumber(a)).ToArray();
            var resultsArray = results.ToArray();
            foreach (var filter in readEngines)
            {
                filter.Read(resultsArray, allNumbers);
            }
            var no = new Card(allNumbers);
            var finalBid = generateEngine.GenerateBet(resultsArray, no, manyNumbers);
            var precision = no.PresenceElements.Where(x => expectedGame.Contains(x.Key)).Select(x => new BetNumber(x.Key, x.Value, 0)).ToArray();

            var finalCardInfo = new Card(allNumbers, precision, finalBid, resultsArray, keyIdentifier, gameIndex);
            return finalCardInfo;
        }
    }
}