using LotteryCrawler;
namespace LotteryCrawler.Core.Strategies.GenerateEngines
{
    public interface IGenerateEngine
    {
        string? Identification { get;  }
        BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements);
    }

    public abstract class BaseEngine //: IGenerateEngine
    {
        protected BetNumber[] _betOptions;

        protected BaseEngine(int maxNumbers)
        {
           _betOptions = Enumerable.Range(1, maxNumbers).Select(n => new BetNumber(n)).ToArray();   
        }
        //public abstract string? Identification { get; }
        //public abstract BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements);
    }       
}
