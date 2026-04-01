using LotteryCrawler.Core.Atributes;

namespace LotteryCrawler.Core.Strategies.GenerateEngines
{
    [Rank(3)]
    public class CombinedGenerator : IGenerateEngine
    {
        public CombinedGenerator(string engineId)
        {
            this.Engine1 = new Lottery("");
            this.Engine2 = new BalancedDecadeEngine("");
            this.EngineId = engineId;
        }

        private static Random _rng = new Random();

        public IGenerateEngine Engine1 { get; }
        internal IGenerateEngine Engine2 { get; }
        private string EngineId { get; }

        public string? Identification => EngineId;

        public BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements)
        {
            var finalResult = new List<BetNumber>();
            var generated1 = this.Engine1.GenerateBet(history, options, maxNumOfElements);
            var generated2 = this.Engine2.GenerateBet(history, options, maxNumOfElements);

            var possibleOptions = generated2.Except(generated1).ToList();
            if (possibleOptions.Any()){
                finalResult.AddRange(generated1.Take(maxNumOfElements - 2));
                for (var index = 0; index < 2; index++)
                {
                    var a = possibleOptions[_rng.Next(possibleOptions.Count())];
                    finalResult.Add(a);
                    possibleOptions.Remove(a);
                }   
                
            }
            return finalResult.ToArray();
        }
    }
}
