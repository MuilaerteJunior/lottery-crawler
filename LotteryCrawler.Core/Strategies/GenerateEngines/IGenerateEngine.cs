namespace LotteryCrawler.Core.Strategies.GenerateEngines
{
    public interface IGenerateEngine
    {
        string? Identification { get; }
        BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements);
    }
}
