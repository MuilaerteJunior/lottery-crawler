namespace LotteryCrawler.Bet.Core
{
    internal interface ILottery
    {
        List<int> CreateNumbersAndProbabilities(BetNumber[] allNumbers);
        int[] GenerateBet(List<int> allNumbers, BetNumber[] allOptions, int maxNumOfElements);
    }
}
