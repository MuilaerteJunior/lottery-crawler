using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml.Linq;

namespace LotteryCrawler.Core.Display
{
    internal class Displayer
    {
        public static void PrintHeader(string header)
        {
            OutputFormatter.PrintSectionTitle(header);
        }
        
        public static void ShowPreciseResults(string[][] outputValues)
        {
            if (outputValues.Length == 0 || outputValues[0].Length <4)
                return;

            string header1 = $"Engine";
            string header2 = "How many bets considered?";
            string header3 = "Expected game";
            string header4 = "Precision Sum";
            string header5 = "Bet generated";
            string header6 = "Precision Sum";
            string header7 = "Matched numbers";

            var rowFormat = "{0,-15} | {1,-30} | {2,-60} | {3,-20}  | {4,-65} | {5,-15} | {6,-50}";
            var headerContent = string.Format(rowFormat,
                           header1,
                           header2,
                           header3,
                           header4,
                           header5,
                           header6,
                           header7);

            Console.WriteLine(headerContent);
            OutputFormatter.PrintLineSeparator(headerContent.Length);

            foreach (var item in outputValues)
            {
                string col1Value = item[0];
                string col2Value = item[1];
                string col3Value = item[2];
                string col4Value = item[3];
                string col5Value = item[4];
                string col6Value = item[5];
                string col7Value = item[6];

                var content = string.Format(rowFormat,
                           col1Value,
                           col2Value,
                           col3Value,
                           col4Value,
                           col5Value,
                           col6Value,
                           col7Value);

                Console.WriteLine(content);
            }
            
            OutputFormatter.PrintLineSeparator();
        }

        private static  string DisplayPrecisionSum(BetNumber[] betNumbers, int totalBetsConsidered)
        {
            var precisionSum = ((betNumbers?.Sum(x => x.PositiveProbability ?? 0m) ?? 0m) / totalBetsConsidered).ToString("0.###");
            return precisionSum;
        }
        public static void ShowStudyAppHelp()
        {
            Console.WriteLine("LotteryCrawler.Bet - Help / Usage");
            Console.WriteLine();
            Console.WriteLine("General syntax:");
            Console.WriteLine("  <option> [value] [attempts]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  q    Quit");
            Console.WriteLine("  s    Study results based on following available engines");
            foreach (var item in (new CoreConfig()).AvailableEngines)
            {
                Console.WriteLine($"  {item.Key}  - {item.Value.Item2.Identification} engine");
            }
            Console.WriteLine("  h    Show this help text");
            Console.WriteLine();
            Console.WriteLine("Parameters:");
            Console.WriteLine("  value:");
            Console.WriteLine("    - Position of a result to search. Nth game");
            Console.WriteLine();            
            Console.WriteLine("Examples:");
            Console.WriteLine("  LotteryCrawler.Bet.exe n 5           -> It will try study the bets trying to match the 5th game");
            Console.WriteLine();
        }
        public static void ShowAppHelp()
        {
            Console.WriteLine("LotteryCrawler.Bet - Help / Usage");
            Console.WriteLine();
            Console.WriteLine("General syntax:");
            Console.WriteLine("  <option> [numberOfBets]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  q    Quit");
            foreach (var item in (new CoreConfig()).AvailableEngines)
            {
                Console.WriteLine($"  {item.Key}    Generate a bet using the {item.Value.Item2.Identification} engine");
            }            
            Console.WriteLine("  h    Show this help text");
            Console.WriteLine();
            Console.WriteLine("Parameters:");
            Console.WriteLine("  numberOfBets: (optional)");
            Console.WriteLine("    - how many bets to generate");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  LotteryCrawler.App.exe n 10          -> Generate 10 bets using n engine");
            Console.WriteLine();
            Console.WriteLine("Notes:");
            Console.WriteLine("  - If no numberOfBets provided, it will be generated just a single bet");
            Console.WriteLine("  - Use lowercase single-character options as shown.");
        }
        public static void ShowPreciseResults(IEnumerable<Card> cardsInfo, bool verbosityMode = false)
        {
            OutputFormatter.PrintLineSeparator();
            if (cardsInfo.Any()) {
                var rowFormat = "{0,-15} | {1,-30} | {2,-60} | {3,-20}  | {4,-65} | {5,-15} | {6,-50}";
                if (verbosityMode)
                {
                    string header1 = $"Engine";
                    string header2 = "How many bets considered?";
                    string header3 = "Expected game";
                    string header4 = "Precision Sum";
                    string header5 = "Bet generated";
                    string header6 = "Precision Sum";
                    string header7 = "Matched numbers";

                    
                    var headerContent = string.Format(rowFormat, header1, header2, header3, header4, header5, header6, header7);

                    Console.WriteLine(headerContent);
                    OutputFormatter.PrintLineSeparator(headerContent.Length);

                    foreach (var item in cardsInfo)
                    {
                        string col1Value = item.EngineName;
                        string col2Value = item.History.Length.ToString();
                        string? col3Value = ToStringPrecision(item.ResultGame!);
                        string col4Value = DisplayPrecisionSum(item.ResultGame, item.History.Length);
                        string? col5Value = ToStringPrecision(item.FinalGame!);
                        string col6Value = DisplayPrecisionSum(item.FinalGame, item.History.Length);
                        string col7Value = string.Join(",", item.MatchedNumbers);

                        var content = string.Format(rowFormat, col1Value, col2Value, col3Value, col4Value, col5Value, col6Value, col7Value);

                        Console.WriteLine(content);
                    }
                }

                OutputFormatter.PrintSectionTitle("Summary of Engines Precision (Match count > 3)");
                var valuableResults = cardsInfo.Where(a => a.MatchedNumbers.Count() > 3).OrderByDescending(x => x.MatchedNumbers.Count());
                foreach (var item in valuableResults)
                {
                    string col1Value = item.EngineName;
                    string col2Value = item.History.Length.ToString();
                    string? col3Value = ToStringSimple(item.ResultGame);
                    string col4Value = DisplayPrecisionSum(item.ResultGame, item.History.Length);
                    string? col5Value = ToStringSimple(item.FinalGame);
                    string col6Value = DisplayPrecisionSum(item.FinalGame, item.History.Length);
                    string col7Value = string.Join(",", item.MatchedNumbers);

                    var content = string.Format(rowFormat, col1Value, col2Value, col3Value, col4Value, col5Value, col6Value, col7Value);
                    Console.WriteLine(content);
                }
                OutputFormatter.PrintLineSeparator();

                var finalSummary = valuableResults.GroupBy(x => x.EngineName)
                                .OrderByDescending(X => X.Max(X => X.MatchedNumbers.Count()))
                                .ThenByDescending(x => x.Count())
                                .Select(x => x);
                var summaryRowFormat= "{0,-20} | {1,-20} | {2,-20}";
                Console.WriteLine(string.Format(summaryRowFormat, "Engine", "Max match", "Bets with more than 3 numbers matched"));
                foreach (var item in finalSummary)
                {
                    Console.WriteLine(string.Format(summaryRowFormat,item.Key, item.Max(a => a.MatchedNumbers.Count()),item.Count()));
                }                
            }
            else  {
                
                Console.Write("Nothing to show");
            }
            OutputFormatter.PrintLineSeparator();
        }

        private static string? ToStringPrecision(BetNumber[] numbers)
        {
            if (numbers == null || numbers.Length <= 0)
                return string.Empty;

            return $"{string.Join(",", numbers.OrderBy(a => a.Number).Select(a => a.Number))} ({string.Join(";", numbers.OrderBy(a => a.Number).Select(a => Math.Round((decimal)(a.PositiveProbability ?? 0m), 3).ToString("0.##")))})";
        }

        private static string? ToStringSimple(BetNumber[] numbers)
        {
            if (numbers == null || numbers.Length <= 0)
                return string.Empty;

            return $"{string.Join(",", numbers.OrderBy(a => a.Number).Select(a => a.Number))}";
        }

    }
}
