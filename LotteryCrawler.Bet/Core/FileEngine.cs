using LotteryCrawler.Bet.Model;

namespace LotteryCrawler.Bet.Core
{
    internal static class FileEngine
    {

        public static void SaveBestInputsInAFile(string executionDate, int numberGamesStudy, int[][] pastResults, int[] winnerBet, List<int> availableNumbersOptions, Queue<MagicalBet> magicalBets)
        {
            //using (var fs = new StreamWriter($"PossibilityPresence_{executionDate}.txt", true))
            //{
            //    fs.WriteLine(string.Concat(index, ","));
            //    foreach (var item in results.Item1)
            //    {
            //        fs.WriteLine("Probability of expected numbers");
            //        fs.WriteLine($"Number {item}. PossibilityPresence {availableNumbersOptions.Count(a => a == item)}");
            //    }
            //}

            //using (var fs = new StreamWriter($"bestParams_{executionDate}.txt", true))
            //{
            //    fs.WriteLine(string.Concat(numberGamesStudy, ","));
            //}

            //using (var fs = new StreamWriter($"bestTryAttempt_{executionDate}.txt", true))
            //{
            //    fs.WriteLine(string.Concat(tryIndex, ","));
            //}
        }

    }
}
