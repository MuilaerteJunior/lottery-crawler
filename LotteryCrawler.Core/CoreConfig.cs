using LotteryCrawler.Core.Strategies.GenerateEngines;
using LotteryCrawler.Core.Strategies.ReadEngines;

public class CoreConfig
{
    public CoreConfig()
    {
        var oldGenerator = new Lottery2();
        var oldGeneratorFixed = new Lottery2();
        var newGenerator = new PredictorEngine();
        var newGeneratorFixed = new PredictorEngine();
        var oldEngines = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new EvaluatePresenceBasedOnDecimals() };
        var newEngine = new List<IReadEngine> { new WeightBasedOnPresence() };
        this.AvailableEngines = new Dictionary<string, Tuple<List<IReadEngine>, IGenerateEngine, int?>>
                {
                    { "n", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(newEngine, newGenerator, default(int?)) },
                    { "o", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, oldGenerator, default(int?)) },
                    { "m1", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new MixedGenerator("Mixed1"), default(int?)) },
                    { "m2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(newEngine, new MixedGenerator("Mixed2"), default(int?)) },
                    { "m3", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines.Union(newEngine).ToList(), new MixedGenerator("Mixed3"), default(int?)) },
                    { "nf", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(newEngine, newGenerator, 306) },
                    { "of", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, oldGenerator, 306) },
                    { "mf1", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new MixedGenerator("(Fixed) Mixed1"), 306) },
                    { "mf2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(newEngine, new MixedGenerator("(Fixed) Mixed2"), 306) },
                    { "mf3", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines.Union(newEngine).ToList(), new MixedGenerator("(Fixed) Mixed3"), 306) },
                };
    }

    public Dictionary<string, Tuple<List<IReadEngine>, IGenerateEngine, int?>> AvailableEngines { get; private set;  }

}