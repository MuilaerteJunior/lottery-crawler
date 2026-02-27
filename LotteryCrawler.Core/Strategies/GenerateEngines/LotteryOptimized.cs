namespace LotteryCrawler.Core.Strategies.GenerateEngines
{
    internal class LotteryOptimized : Lottery
    {
        public override string? Identification => "LotteryOptimized";
        public const int LIMIT_GENERATION = 1000;

        public override BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements)
        {
            var result = base.GenerateBet(history, options, maxNumOfElements);

            if (history.Length >= 4) {
                var mostUnprobable = history.TakeLast(4).SelectMany(a => a).GroupBy(a => a).Where(a => a.Count() >= 2).Select(x => x.Key);
                var currentGeneration = 1;
                while (mostUnprobable.Intersect(result.Select(x => x.Number)).Any() && currentGeneration < LIMIT_GENERATION)
                {
                    result = base.GenerateBet(history, options, maxNumOfElements);
                    currentGeneration++;
                }
            }

            return result;
        }
    }
}
