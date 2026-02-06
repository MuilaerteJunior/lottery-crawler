namespace LotteryCrawler.Core.Strategies.GenerateEngines
{
    public class PredictorEngine : IGenerateEngine
    {
        private static Random _rng = new Random();

        public string? Identification => "Predictor";

        public BetNumber[] GenerateBet(int[][] history, Card options, short maxNumOfElements)
        {
            var deltaPool = CalculateDeltaPool(history);
            var startPool = history.Select(h => h.Min()).ToList();
            BetNumber[]? bestSequence = null;
            decimal highestScore = -1;

            for (int i = 0; i < 10000; i++)
            {
                var candidate = GenerateCandidate(startPool, deltaPool, maxNumOfElements);
                decimal score = ScoreCandidate(candidate, options.PresenceElements);

                if (score > highestScore)
                {
                    highestScore = score;
                    bestSequence = candidate.Select(a => new BetNumber(a, options.PresenceElements[a],0 )).ToArray();
                }
            }

            if (bestSequence == null)
                throw new InvalidOperationException("No valid sequence generated.");

            return bestSequence;
        }

        private static decimal ScoreCandidate(List<int> candidate, Dictionary<int, decimal> options)
        {
            return candidate.Sum(n => options.ContainsKey(n) ? options[n] : 0m);
        }

        private static List<int> GenerateCandidate(List<int> starts, List<int> deltas, short maxNumOfElements)
        {
            var candidate = new List<int> { starts[_rng.Next(starts.Count)] };
            while (candidate.Count < maxNumOfElements)
            {
                int next = candidate.Last() + deltas[_rng.Next(deltas.Count)];
                if (next <= 60 && !candidate.Contains(next)) 
                    candidate.Add(next);
                else 
                    candidate.Add(candidate.Last());
            }
            return candidate.OrderBy(x => x).ToList();
        }

        private static List<int> CalculateDeltaPool(int[][] history)
        {
            var deltas = new List<int>();
            foreach (var draw in history)
            {
                var drawNumbers = draw.OrderBy(x => x).ToArray();
                for (int index = 1; index < drawNumbers.Length; index++) 
                        deltas.Add(drawNumbers[index] - drawNumbers[index - 1]);
            }
            return deltas;
        }
    }
}
