using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
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

        public static void ShowStudyAppResults(int[][] results)
        {
            
            if (results.Length != 0)
            {
                OutputFormatter.PrintSectionTitle("LotteryCrawler.Bet - Showing results");
                var rowFormat = "{0,-4} | {1,-70} ";
                string header1 = $"#";
                string header2 = "Result";
                var headerContent = string.Format(rowFormat, header1, header2);

                Console.WriteLine(headerContent);
                OutputFormatter.PrintLineSeparator(headerContent.Length);
                for (int resultIndex = 0; resultIndex < results.Length; resultIndex++)
                {
                    int[] item = results[resultIndex]; 
                    string result = string.Join(" - ", item);
                    var content = string.Format(rowFormat, resultIndex + 1, result);

                    Console.WriteLine(content);
                }


                OutputFormatter.PrintLineSeparator();
                OutputFormatter.PrintSectionTitle("Digit study");
                var allResults = results.SelectMany(t => t.Select(a => a.ToString("0#")));
                var rightSide = allResults.GroupBy(x => x[1])
                        .OrderByDescending(X => X.Count())
                        .Select(x => $"{x.Key} - Qty: {x.Count()}");
                var leftSide = allResults.GroupBy(x => x[0])
                        .OrderByDescending(X => X.Count())
                        .Select(x => $"{x.Key} - Qty: {x.Count()}");
                Console.WriteLine("Right side:");
                Console.WriteLine(string.Join(Environment.NewLine, rightSide));
                Console.WriteLine("Left side:");
                Console.WriteLine(string.Join(Environment.NewLine, leftSide));

                OutputFormatter.PrintSectionTitle("Average study");
                var rowFormat3Headers = "{0,-4} | {1,-70} | {2,-30}";
                var headerContentAvg = string.Format(rowFormat3Headers, header1, header2, "AVG");
                Console.WriteLine(headerContentAvg);
                for (int resultIndex = 0; resultIndex < results.Length; resultIndex++)
                {
                    int[] item = results[resultIndex];
                    string result = string.Join(" - ", item);
                    var content = string.Format(rowFormat3Headers, resultIndex + 1, result, item.Average());
                    Console.WriteLine(content);
                }

                OutputFormatter.PrintLineSeparator();
                Console.WriteLine($"Min AVG:{results.Min(x => x.Average())}" +
                                  $"Max AVG:{results.Max(x => x.Average())}"+
                                  $"AVG:{results.Average(x => x.Average())}");
                OutputFormatter.PrintLineSeparator();

                OutputFormatter.PrintSectionTitle("Difference study");
                var rowFormat4Headers = "{0,-4} | {1,-70} | {2,-30} | {3,-30}";
                var headerContentDiff = string.Format(rowFormat4Headers, header1, header2, "Diff between elements", "Diff avg");
                double minDiffAvg = 0; double maxDiffAvg = 0;
                Console.WriteLine(headerContentDiff);
                for (int resultIndex = 0; resultIndex < results.Length; resultIndex++)
                {
                    int[] item = results[resultIndex].OrderBy(a => a).ToArray();
                    string result = string.Join(" - ", item);

                    var value = item.Select((elemnt, index) => index == 0 ? elemnt - 0 : elemnt - item[index - 1]);
                    var content = string.Format(rowFormat4Headers, resultIndex + 1, result, string.Join("-", value), value.Average());
                    if (minDiffAvg  == 0 || value.Average() < minDiffAvg )
                        minDiffAvg = value.Average();

                    if( value.Average() > maxDiffAvg )
                        maxDiffAvg = value.Average() ;
                        
                    Console.WriteLine(content);
                }

                OutputFormatter.PrintLineSeparator();
                Console.WriteLine($"Min diff AVG:{minDiffAvg} Max diff AVG:{maxDiffAvg}");
                OutputFormatter.PrintLineSeparator();
            }
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
            Console.WriteLine("  -v:");
            Console.WriteLine("    - It will show detailed information");
            Console.WriteLine("  number:");
            Console.WriteLine("    - how many numbers to consider as a bet simulation");
            Console.WriteLine("  gameIndex:");
            Console.WriteLine("    - It will consider the N-game as the aimed result to predict");
            Console.WriteLine();            
            Console.WriteLine("Examples:");
            Console.WriteLine("  LotteryCrawler.Bet.exe s 8           -> It will generate all bets consider 8 numbers");
            Console.WriteLine("  LotteryCrawler.Bet.exe s -v          -> It will generate all bets consider 6 numbers in detailed mode");
            Console.WriteLine("  LotteryCrawler.Bet.exe s 6 1500      -> It will generate all bets considering 6 numbers that will try to generate the result from 1500th game");
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
            Console.WriteLine("  How Many numbers: (optional - integer)");
            Console.WriteLine("    - It will generate a game considered N numbers ");
            Console.WriteLine("  Batch mode: (optional - integer)");
            Console.WriteLine("    - how many bets to generate");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  LotteryCrawler.App.exe n 6 10          -> Generate 10 bets using n engine");
            Console.WriteLine();
            Console.WriteLine("Notes:");
            Console.WriteLine("  - If no numberOfBets provided, it will be generated just a single bet");
            Console.WriteLine("  - Use lowercase single-character options as shown.");
        }

        public static void ShowPreciseResults(IEnumerable<Card> cardsInfo, bool verbosityMode = false)
        {
            OutputFormatter.PrintLineSeparator();
            if (cardsInfo.Any())
            {
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
                OutputFormatter.PrintSectionTitle("Analysis");
                var info = cardsInfo.Select(c => c.ResultGame.Sum(x => x.Number));
                Console.WriteLine($"Average sum of expected games: {info.Average():0.##}");
                Console.WriteLine($"Min Sum: {info.Min():0.##}");
                Console.WriteLine($"Max sum: {info.Max():0.##}");


                OutputFormatter.PrintSectionTitle("Summary of Engines Precision (Match count > 3)");
                var valuableResults = cardsInfo.Where(a => a.MatchedNumbers.Count() > 3).OrderByDescending(x => x.MatchedNumbers.Count());
                var rowFormatSummary = "{0,-15} | {1,-8} | {2,-25} | {3,-8}  | {4,-25} | {5,-8} | {6,-25}";
                foreach (var item in valuableResults)
                {
                    string col1Value = item.EngineName;
                    string col2Value = item.History.Length.ToString();
                    string? col3Value = ToStringSimple(item.ResultGame);
                    string col4Value = DisplayPrecisionSum(item.ResultGame, item.History.Length);
                    string? col5Value = ToStringSimple(item.FinalGame);
                    string col6Value = DisplayPrecisionSum(item.FinalGame, item.History.Length);
                    string col7Value = string.Join(",", item.MatchedNumbers.OrderBy(x=> x));

                    var content = string.Format(rowFormatSummary, col1Value, col2Value, col3Value, col4Value, col5Value, col6Value, col7Value);
                    Console.WriteLine(content);
                }
                OutputFormatter.PrintLineSeparator();


                OutputFormatter.PrintSectionTitle("Performance summary:");


                var finalSummary = valuableResults.GroupBy(x => x.EngineName)
                                .OrderByDescending(X => X.Max(X => X.MatchedNumbers.Count()))
                                .ThenByDescending(x => x.Count())
                                .Select(x => new DisplaySummaryInfo2
                                {
                                    EngineName = x.Key,
                                    MaxMatchCount = x.Max(a => a.MatchedNumbers.Count()),
                                    ManyEffectiveDraws = x.Count()
                                });

                var summaryRowFormat = "{0,-20} | {1,-20} | {2,-30}| {3,-20}";
                var historyCount = cardsInfo.Max(x => x.History.Length);

                Print(string.Format(summaryRowFormat, "Engine", "Max match", "Bets with > 3 numbers matched", "Precision (%)"), finalSummary, summaryRowFormat, historyCount);

                var finalSummary2 = valuableResults.GroupBy(x => new { Engine = x.EngineName, MatchCount = x.MatchedNumbers.Count(), MissedNumbers = x.ResultGame.Select(x=>x.Number).Except(x.MatchedNumbers).ToList() })
                                .OrderByDescending(g => g.Key.MatchCount)
                                .ThenByDescending(x => x.Sum(z => z.MatchedNumbers.Count()))
                                .Select(x => new DisplaySummaryInfo3
                                {
                                    EngineName = x.Key.Engine,
                                    MatchCount = x.Key.MatchCount,
                                    EffectiveDrawsHaving = x.Count(),
                                    MissedNumbers = string.Join(" - ", x.Key.MissedNumbers)
                                });;

                OutputFormatter.PrintLineSeparator();
                var summaryRowFormat2 = "{0,-20} | {1,-20} | {2,-30} | {3,-20} | {4,-20}";
                Print2(string.Format(summaryRowFormat2, "Engine", "Match count", "Number of batchs having this", "Precision (%)", "Missed Numbers"), finalSummary2, summaryRowFormat2, historyCount);

                var classifyingAlgorithms = valuableResults.GroupBy(x => x.EngineName)
                                .Select(k => new DisplaySummaryInfo
                                {
                                    Summary = k.Key,
                                    Weight =
                                    k.Where(a => a.MatchedNumbers.Count() == 6).Sum(a => a.MatchedNumbers.Count() * 300)
                                    + k.Where(a => a.MatchedNumbers.Count() == 5).Sum(a => a.MatchedNumbers.Count() * 33)
                                    + k.Where(a => a.MatchedNumbers.Count() == 4).Sum(a => a.MatchedNumbers.Count())
                                }).OrderByDescending(x => x.Weight).ToList();

                OutputFormatter.PrintLineSeparator();
                Printout("Top best 5 algorithms 6 * 300 |  5 * 33 | 4 * 1", [.. classifyingAlgorithms.Take(5)]);

                OutputFormatter.PrintLineSeparator();
                Printout("Top 5 worst algorithms:", [.. classifyingAlgorithms.TakeLast(5).OrderBy(x => x.Weight)]);
            }
            else  {
                
                Console.Write("Nothing to show");
            }
            OutputFormatter.PrintLineSeparator();
        }

        private static void Print(string title, IEnumerable<DisplaySummaryInfo2> finalSummary, string summaryRowFormat, int historyCount)
        {
            Console.WriteLine(title);
            foreach (var item in finalSummary)
            {
                decimal precision = ((decimal)item.ManyEffectiveDraws / historyCount);
                Console.WriteLine(string.Format(summaryRowFormat,
                    item.EngineName,
                    item.MaxMatchCount,
                    item.ManyEffectiveDraws,
                    precision.ToString("0.##%")));
            }
        }

        private static void Print2(string title, IEnumerable<DisplaySummaryInfo3> finalSummary, string summaryRowFormat, int historyCount)
        {
            Console.WriteLine(title);
            foreach (var item in finalSummary)
            {
                decimal precision = ((decimal)item.EffectiveDrawsHaving / historyCount);
                Console.WriteLine(string.Format(summaryRowFormat,
                    item.EngineName,
                    item.MatchCount,
                    item.EffectiveDrawsHaving,
                    precision.ToString("0.##%"),
                    item.MissedNumbers));
            }
        }

        private static void Printout(string title, List<DisplaySummaryInfo> classifyingAlgorithms)
        {
            OutputFormatter.PrintSectionTitle(title);
            foreach (var item in classifyingAlgorithms)
            {
                Console.WriteLine(string.Format("{0,-20} | {1,-20} ", item.Summary, item.Weight));
            }
        }

        private class DisplaySummaryInfo
        {
            public string Summary { get; set; } = string.Empty;
            public double Weight{ get; set; }
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

        private class DisplaySummaryInfo2
        {
            public string EngineName { get; set; } = string.Empty;
            public int MaxMatchCount { get; set; }
            public int ManyEffectiveDraws { get; set; }
        }

        private class DisplaySummaryInfo3
        {
            public string EngineName { get; set; } = string.Empty;
            public int MatchCount { get; set; }
            public int EffectiveDrawsHaving { get; set; }
            public string MissedNumbers { get; internal set; } = string.Empty;
        }
    }
}
