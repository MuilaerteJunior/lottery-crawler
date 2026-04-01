using LotteryCrawler.Core.Strategies.GenerateEngines;
using LotteryCrawler.Core.Strategies.ReadEngines;

public class CoreConfig
{
    public CoreConfig()
    {
        var lottery = new Lottery("lottery");
        var optimized = new LotteryOptimized();
        var lottery2 = new Lottery2();
        var lottery3 = new Lottery3();
        var oldEngines = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new EvaluatePresenceBasedOnDecimals() };
        var newEngine = new List<IReadEngine> { new WeightBasedOnPresence() };
        var allengines = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new EvaluatePresenceBasedOnDecimals(), new WeightBasedOnPresence() };
        var duenessEngines = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new DuenessReadEngine() };
        var recentWindowEngines = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new RecentWindowFrequencyReadEngine() };
        var expDecayEngines = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new ExponentialDecayReadEngine() };
        var decimalIntervalEngines = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new EvaluatePresenceBasedOnDecimals(), new IntervalReadEngine() };
        var expDecayIntervalEngines = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new ExponentialDecayReadEngine(), new IntervalReadEngine() };
        var normOldEngines          = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new EvaluatePresenceBasedOnDecimals(), new MinMaxNormalizerReadEngine() };
        var normExpDecayEngines     = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new ExponentialDecayReadEngine(), new MinMaxNormalizerReadEngine() };
        var normExpDecayInterval    = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new ExponentialDecayReadEngine(), new IntervalReadEngine(), new MinMaxNormalizerReadEngine() };
        var normSimilarityExpDecay  = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new SimilarityWeightedReadEngine(), new ExponentialDecayReadEngine(), new MinMaxNormalizerReadEngine() };
        var normFullStack           = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new ExponentialDecayReadEngine(), new IntervalReadEngine(), new SimilarityWeightedReadEngine(), new MinMaxNormalizerReadEngine() };
        var rankAgg = new List<IReadEngine>
        {
            new RankAggregationReadEngine(
                new ReducePresenceBasedOnPreviousResults(),
                new ExponentialDecayReadEngine(),
                new IntervalReadEngine(),
                new SimilarityWeightedReadEngine())
        };
        var rankAggNorm = new List<IReadEngine>
        {
            new RankAggregationReadEngine(
                new ReducePresenceBasedOnPreviousResults(),
                new ExponentialDecayReadEngine(),
                new IntervalReadEngine(),
                new SimilarityWeightedReadEngine()),
            new MinMaxNormalizerReadEngine()
        };
        var coOccurrenceNorm = new List<IReadEngine>
        {
            new ReducePresenceBasedOnPreviousResults(),
            new ExponentialDecayReadEngine(),
            new CoOccurrenceReadEngine(10),
            new MinMaxNormalizerReadEngine()
        };
        var coOccurrenceFullNorm = new List<IReadEngine>
        {
            new ReducePresenceBasedOnPreviousResults(),
            new ExponentialDecayReadEngine(),
            new CoOccurrenceReadEngine(10),
            new SimilarityWeightedReadEngine(200),
            new MinMaxNormalizerReadEngine()
        };

        var trendEngines = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new EvaluatePresenceBasedOnDecimals(), new FrequencyTrendReadEngine() };

        this.AvailableEngines = new Dictionary<string, Tuple<List<IReadEngine>, IGenerateEngine, int?>>
                {
                    { "o", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, lottery, default(int?)) },
                    { "ozzzz", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new Lottery("ozzzz"), 306) },
                    //{ "on", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new NewLottery(), default(int?)) },
                    { "zz", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(new List<IReadEngine>() { new TestingResults() } , new Lottery("zz") {}, default(int?)) },
                    { "zzz", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(
                        new List<IReadEngine>() { new TestingResults() ,  new ReducePresenceBasedOnPreviousResults(), new EvaluatePresenceBasedOnDecimals() } , 
                        new Lottery("zzz"), default(int?)) },
                    { "kkk", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(new List<IReadEngine>() { new TestingResults() ,  new ReducePresenceBasedOnPreviousResults(), new EvaluatePresenceBasedOnDecimals() } , new SimpleLottery("kkk"), default(int?)) },                    
                    { "wpzzzz", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines,    new WeightedProbabilityEngine("wpzzzz"), default(int?)) },
                    { "wpzzxcasd", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(recentWindowEngines,    new Lottery("wpzzxcasd"), default(int?)) },
                    { "bd_nois", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines,new BalancedDecadeEngine("bd_nois"), default(int?)) },
                    { "bd_nois222", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(expDecayIntervalEngines,new Lottery("bd_nois222"), default(int?)) },
                    { "cp3",  new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines,  new ConsensusPoolLotteryEngine("cp3"),  default(int?)) },
                    { "cp4",  new Tuple<List<IReadEngine>, IGenerateEngine, int?>(trendEngines.Union(oldEngines).ToList(),  new ConsensusPoolLotteryEngine("cp4"),  default(int?)) },
                    //{ "cp5",  new Tuple<List<IReadEngine>, IGenerateEngine, int?>(trendEngines.Union(oldEngines).ToList(),  new NewLottery4(),  default(int?)) },
                    //{ "cp65",  new Tuple<List<IReadEngine>, IGenerateEngine, int?>(trendEngines,  new NewLottery5(),  default(int?)) },
                    { "o1", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, lottery2, default(int?)) },
                    { "l", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, lottery3, default(int?)) },
                    { "l2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, optimized, default(int?)) },
                    { "c", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new CombinedGenerator("Combined"), default(int?)) },
                    { "m1", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new MixedGenerator("Mixed1"), default(int?)) },
                    { "m2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(newEngine, new MixedGenerator("Mixed2"), default(int?)) },
                    { "m3", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines.Union(newEngine).ToList(), new MixedGenerator("Mixed3"), default(int?)) },
                    //{ "of", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, lottery, 306) },
                    { "mf1", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new MixedGenerator("(Fixed) Mixed1"), 306) },
                    { "mf2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(newEngine, new MixedGenerator("(Fixed) Mixed2"), 306) },
                    { "mf3", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines.Union(newEngine).ToList(), new MixedGenerator("(Fixed) Mixed3"), 306) },
                    // --- New weighted probability engines ---
                    { "wp",  new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines,            new WeightedProbabilityEngine("wp"),  default(int?)) },
                    { "wp2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(duenessEngines,         new WeightedProbabilityEngine("wp2"), default(int?)) },
                    { "wp3", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(recentWindowEngines,    new WeightedProbabilityEngine("wp3"), default(int?)) },
                    { "wp4", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(expDecayEngines,        new WeightedProbabilityEngine("wp4"), default(int?)) },
                    { "wp5", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(decimalIntervalEngines, new WeightedProbabilityEngine("wp5"), default(int?)) },
                    { "wp6", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(expDecayIntervalEngines,new WeightedProbabilityEngine("wp6"), default(int?)) },
                    { "wpf", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines,            new WeightedProbabilityEngine("wpf"), 306) },
                    // --- New balanced decade engines ---
                    { "bd",  new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines,            new BalancedDecadeEngine("bd"),  default(int?)) },
                    { "bd2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(duenessEngines,         new BalancedDecadeEngine("bd2"), default(int?)) },
                    { "bd3", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(recentWindowEngines,    new BalancedDecadeEngine("bd3"), default(int?)) },
                    { "bd4", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(expDecayEngines,        new BalancedDecadeEngine("bd4"), default(int?)) },
                    { "bd5", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(decimalIntervalEngines, new BalancedDecadeEngine("bd5"), default(int?)) },
                    { "bd6", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(expDecayIntervalEngines,new BalancedDecadeEngine("bd6"), default(int?)) },
                    { "bdf", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines,            new BalancedDecadeEngine("bdf"), 306) },
                    // --- Sum-constrained weighted engine ---
                    { "sc",  new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines,     new SumConstrainedWeightedEngine(label: "sc"),  default(int?)) },
                    { "sc2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(expDecayEngines, new SumConstrainedWeightedEngine(label: "sc2"), default(int?)) },
                    // --- Parity balanced engine ---
                    { "pb",  new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines,     new ParityBalancedEngine("pb"),  default(int?)) },
                    { "pb2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(expDecayEngines, new ParityBalancedEngine("pb2"), default(int?)) },

                    // ============================================================
                    // --- TopN: deterministic highest-score selection (zero randomness) ---
                    // Naming: "tn" prefix = TopN generator
                    // ============================================================
                    { "tn",  new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normOldEngines,         new WeightedProbabilityTopNEngine("tn"),  default(int?)) },
                    { "tn2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normExpDecayEngines,    new WeightedProbabilityTopNEngine("tn2"), default(int?)) },
                    { "tn3", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normExpDecayInterval,   new WeightedProbabilityTopNEngine("tn3"), default(int?)) },
                    { "tn4", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normSimilarityExpDecay, new WeightedProbabilityTopNEngine("tn4"), default(int?)) },
                    { "tn5", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normFullStack,          new WeightedProbabilityTopNEngine("tn5"), default(int?)) },

                    // ============================================================
                    // --- Consensus: Monte Carlo voting (approaches TopN with diversity) ---
                    // Naming: "cn" prefix = Consensus generator
                    // ============================================================
                    { "cn",  new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normOldEngines,         new WeightedProbabilityConsensusEngine(label: "cn"),  default(int?)) },
                    { "cn2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normExpDecayEngines,    new WeightedProbabilityConsensusEngine(label: "cn2"), default(int?)) },
                    { "cn3", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normExpDecayInterval,   new WeightedProbabilityConsensusEngine(label: "cn3"), default(int?)) },
                    { "cn4", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normSimilarityExpDecay, new WeightedProbabilityConsensusEngine(label: "cn4"), default(int?)) },
                    { "cn5", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normFullStack,          new WeightedProbabilityConsensusEngine(label: "cn5"), default(int?)) },

                    // --- Rank-aggregation (Borda Count) ---
                    { "ra",  new Tuple<List<IReadEngine>, IGenerateEngine, int?>(rankAgg,             new WeightedProbabilityTopNEngine("ra"),         default(int?)) },
                    { "ran", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(rankAggNorm,         new WeightedProbabilityConsensusEngine(label: "ran"), default(int?)) },
                    { "ra2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(rankAggNorm,         new WeightedProbabilityTopNEngine("ra2"),        default(int?)) },

                    // --- Co-occurrence ---
                    { "co",  new Tuple<List<IReadEngine>, IGenerateEngine, int?>(coOccurrenceNorm,     new WeightedProbabilityTopNEngine("co"),          default(int?)) },
                    { "coc", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(coOccurrenceNorm,     new WeightedProbabilityConsensusEngine(label: "coc"), default(int?)) },
                    { "co2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(coOccurrenceFullNorm, new WeightedProbabilityTopNEngine("co2"),         default(int?)) },

                    // --- Multi-pipeline ensemble ---
                    { "en",  new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines,    new MultiPipelineEnsembleEngine(label: "en"),  default(int?)) },
                    { "en2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(rankAggNorm,   new MultiPipelineEnsembleEngine(label: "en2"), default(int?)) },
                    { "en3", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normFullStack, new MultiPipelineEnsembleEngine(label: "en3"), default(int?)) },

                    // --- Consensus Pool + Lottery-style (pool quality improvement) ---
                    { "cp",   new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines,    new ConsensusPoolLotteryEngine("cp"),   default(int?)) },
                    { "cp2",  new Tuple<List<IReadEngine>, IGenerateEngine, int?>(trendEngines,  new ConsensusPoolLotteryEngine("cp2"),  default(int?)) },

                    // --- Adaptive Lottery (pool size sweep) ---
                    { "al25", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new AdaptiveLotteryEngine(25, "al25"), default(int?)) },
                    { "al40", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new AdaptiveLotteryEngine(40, "al40"), default(int?)) },
                    { "al50", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new AdaptiveLotteryEngine(50, "al50"), default(int?)) },
                    { "al60", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new AdaptiveLotteryEngine(60, "al60"), default(int?)) },

                    // --- Frequency trend + original Lottery ---
                    { "ft",  new Tuple<List<IReadEngine>, IGenerateEngine, int?>(trendEngines, lottery, default(int?)) },
                    { "ft2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(trendEngines, new AdaptiveLotteryEngine(40, "ft2"), default(int?)) },
                };
    }

    public Dictionary<string, Tuple<List<IReadEngine>, IGenerateEngine, int?>> AvailableEngines { get; private set;  }

}