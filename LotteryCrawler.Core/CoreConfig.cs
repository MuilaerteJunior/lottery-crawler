using LotteryCrawler.Core.Strategies.GenerateEngines;
using LotteryCrawler.Core.Strategies.ReadEngines;

public class CoreConfig
{
    public Dictionary<string, Tuple<List<IReadEngine>, IGenerateEngine, int?>> AvailableEngines { get; private set; } = new Dictionary<string, Tuple<List<IReadEngine>, IGenerateEngine, int?>>();
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
        var normOldEngines = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new EvaluatePresenceBasedOnDecimals(), new MinMaxNormalizerReadEngine() };
        var normExpDecayEngines = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new ExponentialDecayReadEngine(), new MinMaxNormalizerReadEngine() };
        var normExpDecayInterval = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new ExponentialDecayReadEngine(), new IntervalReadEngine(), new MinMaxNormalizerReadEngine() };
        var normSimilarityExpDecay = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new SimilarityWeightedReadEngine(), new ExponentialDecayReadEngine(), new MinMaxNormalizerReadEngine() };
        var normFullStack = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new ExponentialDecayReadEngine(), new IntervalReadEngine(), new SimilarityWeightedReadEngine(), new MinMaxNormalizerReadEngine() };
        var top5 = new List<IReadEngine>
        {
            new CoOccurrenceReadEngine(),
            //new RankAggregationReadEngine(),
            new SimilarityWeightedReadEngine(),
            new DuenessReadEngine(),
            new FrequencyTrendReadEngine(),
        };
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
        var testing = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new EvaluatePresenceBasedOnDecimals(), new RecentWindowFrequencyReadEngine(6) };
        var testin2g = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new EvaluatePresenceBasedOnDecimals(), new RecentWindowFrequencyImprovedReadEngine(6) };
        var trendEngines = new List<IReadEngine> { new ReducePresenceBasedOnPreviousResults(), new EvaluatePresenceBasedOnDecimals(), new FrequencyTrendReadEngine() };
        var allCreatedReadEngines = oldEngines
           .Concat(newEngine)
           .Concat(allengines)
           .Concat(duenessEngines)
           .Concat(recentWindowEngines)
           .Concat(expDecayEngines)
           .Concat(decimalIntervalEngines)
           .Concat(expDecayIntervalEngines)
           .Concat(normOldEngines)
           .Concat(normExpDecayEngines)
           .Concat(normExpDecayInterval)
           .Concat(normSimilarityExpDecay)
           .Concat(normFullStack)
           .Concat(rankAgg)
           .Concat(rankAggNorm)
           .Concat(coOccurrenceNorm)
           .Concat(coOccurrenceFullNorm)
           .Concat(testing)
           .Concat(trendEngines)
           .ToList();

        AddLotteryEngines(lottery, oldEngines, top5, testing, testin2g);
        AddCombinedEngine(oldEngines);
        AddMixedGenerators(lottery, oldEngines, newEngine);
        AddWeightedProbabilityEngines(oldEngines, duenessEngines, recentWindowEngines, expDecayEngines, decimalIntervalEngines, expDecayIntervalEngines);
        AddBalancedDecade(oldEngines, duenessEngines, recentWindowEngines, expDecayEngines, decimalIntervalEngines, expDecayIntervalEngines);
        AddSumConstrainedWeighted(oldEngines, expDecayEngines);
        AddParityBalanced(oldEngines, expDecayEngines);
        AddTopN(normOldEngines, normExpDecayEngines, normExpDecayInterval, normSimilarityExpDecay, normFullStack);
        AddConsensusMonteCarlo(normOldEngines, normExpDecayEngines, normExpDecayInterval, normSimilarityExpDecay, normFullStack);
        AddRankAggregation(rankAgg, rankAggNorm);
        AddCoOccurrence(coOccurrenceNorm, coOccurrenceFullNorm);
        AddMultipipeline(oldEngines, normFullStack, rankAggNorm);
        AddConsensusPool(oldEngines, trendEngines);
        AddAdaptativeLottery(oldEngines);
        AddFrequency(lottery, trendEngines);
    }

    private void AddCombinedEngine(List<IReadEngine> oldEngines)
    {
        this.AvailableEngines.Add("c", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new CombinedGenerator("Combined"), default(int?)));
    }

    private void AddMixedGenerators(Lottery lottery, List<IReadEngine> oldEngines, List<IReadEngine> newEngine)
    {
        this.AvailableEngines.Add("m1", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new MixedGenerator("Mixed1"), default(int?)));
        this.AvailableEngines.Add("m2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(newEngine, new MixedGenerator("Mixed2"), default(int?)));
        this.AvailableEngines.Add("m3", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines.Union(newEngine).ToList(), new MixedGenerator("Mixed3"), default(int?)));
        this.AvailableEngines.Add("of", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, lottery, 306));
        this.AvailableEngines.Add("mf1", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new MixedGenerator("(Fixed) Mixed1"), 306));
        this.AvailableEngines.Add("mf2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(newEngine, new MixedGenerator("(Fixed) Mixed2"), 306));
        this.AvailableEngines.Add("mf3", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines.Union(newEngine).ToList(), new MixedGenerator("(Fixed) Mixed3"), 306));
        //this.AvailableEngines.Add("o1", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, lottery2, default(int?)));
        //this.AvailableEngines.Add("l", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, lottery3, default(int?)));
        //this.AvailableEngines.Add("l2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, optimized, default(int?)));
        this.AvailableEngines.Add("kkk", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(new List<IReadEngine>() { new TestingResults(), new ReducePresenceBasedOnPreviousResults(), new EvaluatePresenceBasedOnDecimals() }, new SimpleLottery("kkk"), default(int?)));
    }

    private void AddLotteryEngines(Lottery lottery, List<IReadEngine> oldEngines, List<IReadEngine> top5, List<IReadEngine> testing, List<IReadEngine> testin2g)
    {
        this.AvailableEngines.Add("abc", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(testing, lottery, default(int?)));
        this.AvailableEngines.Add("oioioioi", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(testin2g, lottery, default(int?)));
        this.AvailableEngines.Add("o", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, lottery, default(int?)));
        this.AvailableEngines.Add("o2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(top5, lottery, default(int?)));
        //this.AvailableEngines.Add("ozzzz", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(allCreatedReadEngines, new Lottery("ozzzz"), 306));
        this.AvailableEngines.Add("zz", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(new List<IReadEngine>() { new TestingResults() }, new Lottery("zz"), default(int?)));
        this.AvailableEngines.Add("zzz", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(new List<IReadEngine>() { new TestingResults(), new ReducePresenceBasedOnPreviousResults(), new EvaluatePresenceBasedOnDecimals() }, new Lottery("zzz"), default(int?)));
        //this.AvailableEngines.Add("wpzzxcasd", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(recentWindowEngines, new Lottery("wpzzxcasd"), default(int?)));
        //this.AvailableEngines.Add("bd_nois222", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(expDecayIntervalEngines, new Lottery("bd_nois222"), default(int?)));
    }

    private void AddWeightedProbabilityEngines(List<IReadEngine> oldEngines, List<IReadEngine> duenessEngines, List<IReadEngine> recentWindowEngines, List<IReadEngine> expDecayEngines, List<IReadEngine> decimalIntervalEngines, List<IReadEngine> expDecayIntervalEngines)
    {
        // --- New weighted probability engines ---
        this.AvailableEngines.Add("wp", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new WeightedProbabilityEngine("wp"), default(int?)));
        this.AvailableEngines.Add("wp2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(duenessEngines, new WeightedProbabilityEngine("wp2"), default(int?)));
        this.AvailableEngines.Add("wp3", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(recentWindowEngines, new WeightedProbabilityEngine("wp3"), default(int?)));
        this.AvailableEngines.Add("wp4", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(expDecayEngines, new WeightedProbabilityEngine("wp4"), default(int?)));
        this.AvailableEngines.Add("wp5", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(decimalIntervalEngines, new WeightedProbabilityEngine("wp5"), default(int?)));
        this.AvailableEngines.Add("wp6", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(expDecayIntervalEngines, new WeightedProbabilityEngine("wp6"), default(int?)));
        this.AvailableEngines.Add("wpf", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new WeightedProbabilityEngine("wpf"), 306));
        this.AvailableEngines.Add("wpzzzz", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new WeightedProbabilityEngine("wpzzzz"), default(int?)));
    }

    private void AddBalancedDecade(List<IReadEngine> oldEngines, List<IReadEngine> duenessEngines, List<IReadEngine> recentWindowEngines, List<IReadEngine> expDecayEngines, List<IReadEngine> decimalIntervalEngines, List<IReadEngine> expDecayIntervalEngines)
    {
        // --- New balanced decade engines ---
        this.AvailableEngines.Add("bd", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new BalancedDecadeEngine("bd"), default(int?)));
        this.AvailableEngines.Add("bd2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(duenessEngines, new BalancedDecadeEngine("bd2"), default(int?)));
        this.AvailableEngines.Add("bd3", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(recentWindowEngines, new BalancedDecadeEngine("bd3"), default(int?)));
        this.AvailableEngines.Add("bd4", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(expDecayEngines, new BalancedDecadeEngine("bd4"), default(int?)));
        this.AvailableEngines.Add("bd5", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(decimalIntervalEngines, new BalancedDecadeEngine("bd5"), default(int?)));
        this.AvailableEngines.Add("bd6", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(expDecayIntervalEngines, new BalancedDecadeEngine("bd6"), default(int?)));
        this.AvailableEngines.Add("bdf", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new BalancedDecadeEngine("bdf"), 306));
        this.AvailableEngines.Add("bd_nois", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new BalancedDecadeEngine("bd_nois"), default(int?)));
    }

    private void AddSumConstrainedWeighted(List<IReadEngine> oldEngines, List<IReadEngine> expDecayEngines)
    {
        // --- Sum-constrained weighted engine ---
        this.AvailableEngines.Add("sc", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new SumConstrainedWeightedEngine(label: "sc"), default(int?)));
        this.AvailableEngines.Add("sc2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(expDecayEngines, new SumConstrainedWeightedEngine(label: "sc2"), default(int?)));
    }

    private void AddParityBalanced(List<IReadEngine> oldEngines, List<IReadEngine> expDecayEngines)
    {
        // --- Parity balanced engine ---
        this.AvailableEngines.Add("pb", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new ParityBalancedEngine("pb"), default(int?)));
        this.AvailableEngines.Add("pb2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(expDecayEngines, new ParityBalancedEngine("pb2"), default(int?)));
    }

    private void AddTopN(List<IReadEngine> normOldEngines, List<IReadEngine> normExpDecayEngines, List<IReadEngine> normExpDecayInterval, List<IReadEngine> normSimilarityExpDecay, List<IReadEngine> normFullStack)
    {
        // ============================================================
        // --- TopN: deterministic highest-score selection (zero randomness) ---
        // Naming: "tn" prefix = TopN generator
        // ============================================================
        this.AvailableEngines.Add("tn", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normOldEngines, new WeightedProbabilityTopNEngine("tn"), default(int?)));
        this.AvailableEngines.Add("tn2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normExpDecayEngines, new WeightedProbabilityTopNEngine("tn2"), default(int?)));
        this.AvailableEngines.Add("tn3", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normExpDecayInterval, new WeightedProbabilityTopNEngine("tn3"), default(int?)));
        this.AvailableEngines.Add("tn4", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normSimilarityExpDecay, new WeightedProbabilityTopNEngine("tn4"), default(int?)));
        this.AvailableEngines.Add("tn5", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normFullStack, new WeightedProbabilityTopNEngine("tn5"), default(int?)));

        this.AvailableEngines.Add("sera", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(new List<IReadEngine>() { new TestingResults() }, new TopPriorityEngine("sera"), default(int?)));
    }

    private void AddConsensusMonteCarlo(List<IReadEngine> normOldEngines, List<IReadEngine> normExpDecayEngines, List<IReadEngine> normExpDecayInterval, List<IReadEngine> normSimilarityExpDecay, List<IReadEngine> normFullStack)
    {
        // ============================================================
        // --- Consensus: Monte Carlo voting (approaches TopN with diversity) ---
        // Naming: "cn" prefix = Consensus generator
        // ============================================================
        this.AvailableEngines.Add("cn", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normOldEngines, new WeightedProbabilityConsensusEngine(label: "cn"), default(int?)));
        this.AvailableEngines.Add("cn2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normExpDecayEngines, new WeightedProbabilityConsensusEngine(label: "cn2"), default(int?)));
        this.AvailableEngines.Add("cn3", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normExpDecayInterval, new WeightedProbabilityConsensusEngine(label: "cn3"), default(int?)));
        this.AvailableEngines.Add("cn4", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normSimilarityExpDecay, new WeightedProbabilityConsensusEngine(label: "cn4"), default(int?)));
        this.AvailableEngines.Add("cn5", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normFullStack, new WeightedProbabilityConsensusEngine(label: "cn5"), default(int?)));
    }

    private void AddRankAggregation(List<IReadEngine> rankAgg, List<IReadEngine> rankAggNorm)
    {
        // --- Rank-aggregation (Borda Count) ---
        this.AvailableEngines.Add("ra", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(rankAgg, new WeightedProbabilityTopNEngine("ra"), default(int?)));
        this.AvailableEngines.Add("ran", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(rankAggNorm, new WeightedProbabilityConsensusEngine(label: "ran"), default(int?)));
        this.AvailableEngines.Add("ra2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(rankAggNorm, new WeightedProbabilityTopNEngine("ra2"), default(int?)));
    }

    private void AddCoOccurrence(List<IReadEngine> coOccurrenceNorm, List<IReadEngine> coOccurrenceFullNorm)
    {
        // --- Co-occurrence ---
        this.AvailableEngines.Add("co", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(coOccurrenceNorm, new WeightedProbabilityTopNEngine("co"), default(int?)));
        this.AvailableEngines.Add("coc", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(coOccurrenceNorm, new WeightedProbabilityConsensusEngine(label: "coc"), default(int?)));
        this.AvailableEngines.Add("co2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(coOccurrenceFullNorm, new WeightedProbabilityTopNEngine("co2"), default(int?)));
    }

    private void AddMultipipeline(List<IReadEngine> oldEngines, List<IReadEngine> normFullStack, List<IReadEngine> rankAggNorm)
    {
        // --- Multi-pipeline ensemble ---
        this.AvailableEngines.Add("en", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new MultiPipelineEnsembleEngine(label: "en"), default(int?)));
        this.AvailableEngines.Add("en2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(rankAggNorm, new MultiPipelineEnsembleEngine(label: "en2"), default(int?)));
        this.AvailableEngines.Add("en3", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(normFullStack, new MultiPipelineEnsembleEngine(label: "en3"), default(int?)));
    }

    private void AddConsensusPool(List<IReadEngine> oldEngines, List<IReadEngine> trendEngines)
    {
        // --- Consensus Pool + Lottery-style (pool quality improvement) ---
        this.AvailableEngines.Add("cp", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new ConsensusPoolLotteryEngine("cp"), default(int?)));
        this.AvailableEngines.Add("cp2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(trendEngines, new ConsensusPoolLotteryEngine("cp2"), default(int?)));
        this.AvailableEngines.Add("cp3", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new ConsensusPoolLotteryEngine("cp3"), default(int?)));
        this.AvailableEngines.Add("cp4", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(trendEngines.Union(oldEngines).ToList(), new ConsensusPoolLotteryEngine("cp4"), default(int?)));
    }

    private void AddAdaptativeLottery(List<IReadEngine> oldEngines)
    {
        // --- Adaptive Lottery (pool size sweep) ---
        this.AvailableEngines.Add("al25", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new AdaptiveLotteryEngine(25, "al25"), default(int?)));
        this.AvailableEngines.Add("al40", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new AdaptiveLotteryEngine(40, "al40"), default(int?)));
        this.AvailableEngines.Add("al50", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new AdaptiveLotteryEngine(50, "al50"), default(int?)));
        this.AvailableEngines.Add("al60", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(oldEngines, new AdaptiveLotteryEngine(60, "al60"), default(int?)));
    }

    private void AddFrequency(Lottery lottery, List<IReadEngine> trendEngines)
    {
        // --- Frequency trend + original Lottery ---
        this.AvailableEngines.Add("ft", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(trendEngines, lottery, default(int?)));
        this.AvailableEngines.Add("ft2", new Tuple<List<IReadEngine>, IGenerateEngine, int?>(trendEngines, new AdaptiveLotteryEngine(40, "ft2"), default(int?)));
    }   

}