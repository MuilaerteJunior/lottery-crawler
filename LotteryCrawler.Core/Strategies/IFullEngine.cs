using LotteryCrawler.Core.Strategies.GenerateEngines;
using LotteryCrawler.Core.Strategies.ReadEngines;

namespace LotteryCrawler.Core.Strategies
{
    public interface IFullEngine : IGenerateEngine, IReadEngine
    {
    }

}
