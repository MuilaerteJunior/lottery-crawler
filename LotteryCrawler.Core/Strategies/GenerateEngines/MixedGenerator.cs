namespace LotteryCrawler.Core.Strategies.GenerateEngines
{
    public class MixedGenerator: IGenerateEngine
    {
        public MixedGenerator(string engineId)
        {
            this.Engine1 = new PredictorEngine();
            this.Engine2 = new Lottery();
            this.EngineId = engineId;
        }

        private static Random _rng = new Random();

        public PredictorEngine Engine1 { get; }
        internal Lottery Engine2 { get; }
        private string EngineId { get; }

        public string? Identification => EngineId;

        public BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements)
        {
            var finalResult = new List<BetNumber>();
            while (finalResult.Count < maxNumOfElements)
            {
                var generated1 = this.Engine1.GenerateBet(history, options, maxNumOfElements);
                var generated2 = this.Engine2.GenerateBet(history, options, maxNumOfElements);

                var finalNumbers = generated1.Union(generated2).Distinct();
                foreach (var number in finalNumbers)
                {
                    if (finalResult.Count < maxNumOfElements && !finalResult.Any(n => n.Number == number.Number))
                        finalResult.Add(number);
                }
            }

            return finalResult.ToArray();
        }
    }
}
