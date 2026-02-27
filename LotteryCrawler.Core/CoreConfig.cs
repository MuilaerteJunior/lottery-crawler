using LotteryCrawler.Core.Strategies.GenerateEngines;
using LotteryCrawler.Core.Strategies.ReadEngines;

public class CoreConfig
{
    public CoreConfig()
    {
        var lottery = new Lottery();
        var optimized = new LotteryOptimized();
        var lottery2 = new Lottery2();
        var lottery3 = new Lottery3();
        var oldEngines = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new EvaluatePresenceBasedOnDecimals() };
        var newEngine = new List<IReadEngine> { new WeightBasedOnPresence() };
        var allengines = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new EvaluatePresenceBasedOnDecimals(), new WeightBasedOnPresence() };
        this.AvailableEngines = new Dictionary<string, Tuple<List<IReadEngine>, IGenerateEngine, int?>>
                {
                    { "o", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, lottery, default(int?)) },
                    { "o1", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, lottery2, default(int?)) },
                    { "l", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, lottery3, default(int?)) },
                    { "l2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, optimized, default(int?)) },
                    { "c", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new CombinedGenerator("Combined"), default(int?)) },
                    { "m1", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new MixedGenerator("Mixed1"), default(int?)) },
                    { "m2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(newEngine, new MixedGenerator("Mixed2"), default(int?)) },
                    { "m3", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines.Union(newEngine).ToList(), new MixedGenerator("Mixed3"), default(int?)) },
                    { "of", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, lottery, 306) },
                    { "mf1", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new MixedGenerator("(Fixed) Mixed1"), 306) },
                    { "mf2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(newEngine, new MixedGenerator("(Fixed) Mixed2"), 306) },
                    { "mf3", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines.Union(newEngine).ToList(), new MixedGenerator("(Fixed) Mixed3"), 306) },
                };
    }

    public Dictionary<string, Tuple<List<IReadEngine>, IGenerateEngine, int?>> AvailableEngines { get; private set;  }

}