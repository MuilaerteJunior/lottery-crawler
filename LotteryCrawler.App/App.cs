// See https://aka.ms/new-console-template for more information
using LotteryCrawler.Core;
using LotteryCrawler.Core.Crawlers;
using LotteryCrawler.Core.Display;
using LotteryCrawler.Core.Model;
using LotteryCrawler.Core.Strategies.GenerateEngines;
using LotteryCrawler.Core.Strategies.ReadEngines;
using Microsoft.Extensions.Options;
using System.Diagnostics.Eventing.Reader;

namespace LotteryCrawler.App
{
    public class App
    {
        private readonly ConfigVisualizationOptions _viewConfig;
        private readonly LotteryService<ApostaDTO> _lottery;
        private const string MENU_OPTION_HELP = "h";
        private const string MENU_OPTION_QUIT = "q";
        private static string[] _menuOptions = { MENU_OPTION_HELP, MENU_OPTION_QUIT };


        public App(IOptions<ConfigVisualizationOptions> config, LotteryService<ApostaDTO> _lott)
        {
            _viewConfig = config.Value;
            _lottery = _lott;
        }

        private record UserInput(string optionMenu, int howManyBets);
        private static UserInput ProcessArgs(string[] args, CoreConfig core)
        {
            var optionMenu = args[0] as string;
            if (optionMenu == null)
                return new UserInput("?", -1);

            int howManyBets = 0;
            string gameToFind = string.Empty;
            if (args.Length >= 2)
            {
                if (!Int32.TryParse(args[1], out howManyBets))
                    gameToFind = args[1];
            }

            return new UserInput(optionMenu.ToLower(), howManyBets);
        }

        public void Run(string[] args)
        {
            Console.WriteLine("Lottery...");            
            if (args != null && args.Length > 0)
            {
                Console.WriteLine("Starting...");
                var msEngine = new MegaSenaEngine(_lottery);
                int[][] results = msEngine.GetPreviousResults();
                var core = new CoreConfig();
                short numberCount = 6;
                var userArgs = ProcessArgs(args, core);
                if (userArgs != null)
                {                    
                    switch (userArgs.optionMenu)
                    {
                        case MENU_OPTION_HELP: Displayer.ShowAppHelp(); break;
                        case MENU_OPTION_QUIT:
                            break;
                        default:
                            if ( core.AvailableEngines.TryGetValue(userArgs.optionMenu, out var item))
                            {
                                if ( userArgs.howManyBets > 0)
                                    GenerateBatch(results, userArgs, item.Item1, item.Item2, numberCount);
                                else
                                    GenerateBetMenu(results, item.Item1, item.Item2, numberCount);
                            }
                            else
                                Console.WriteLine($"Choose a valid option between {string.Join("/",_menuOptions)}");
                            break;
                    }
                } else
                    Console.WriteLine($"Choose a valid option between {string.Join("/", _menuOptions)}");
            }
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static void GenerateBatch(int[][] results, UserInput userArgs, List<IReadEngine> oldEngines, IGenerateEngine generateEngine, short manyNumbers)
        {
            if (userArgs.howManyBets > 0)
            {
                Console.WriteLine($"Option: Generating bets in batch mode {userArgs.howManyBets}");
                var generatedGames = new Queue<MagicalBet>();
                for (int i = 0; i < userArgs.howManyBets; i++)
                {
                    generatedGames.Enqueue(GenerateABet(results, oldEngines, generateEngine, manyNumbers));
                }
                foreach (var game in generatedGames)
                {
                    Console.WriteLine("Generated bet: " + game.ToString());
                }
            }
            else
                Console.WriteLine("Missing how many bets to generate config ");
        }
        
        private static void GenerateBetMenu(int[][] results, List<IReadEngine> readEngines, IGenerateEngine generatorEngine, short manyNumbers)
        {
            Console.WriteLine("Option: Generating a bet in new mode");
            var newModeBet = GenerateABet(results, readEngines, generatorEngine, manyNumbers);
            Console.WriteLine("Generated bet: " + newModeBet.ToString());
        }

        static MagicalBet GenerateABet(int[][] results, List<IReadEngine> readEngines, IGenerateEngine generateEngine, short manyNumbers)
        {
            var allNumbers = Enumerable.Range(1, 60).Select(a => new BetNumber(a)).ToArray();                
            foreach (var filter in readEngines)
            {
                filter.Read(results, allNumbers);
            }
            var no = new Card(allNumbers);
            var finalBid = generateEngine.GenerateBet(results, no, manyNumbers);
            return new MagicalBet(finalBid);
        }
    }
}