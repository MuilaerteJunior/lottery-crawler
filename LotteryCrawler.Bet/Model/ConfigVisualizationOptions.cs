namespace LotteryCrawler
{
    public class ConfigVisualizationOptions { 
        public bool ShowStudiedGames { get; set; }
        public bool ShowTop10MostFrequentOnTries{ get; set; }
        public bool ShowTop10LessFrequentOnTries { get; set; }
        public bool ShowProbabilityOfEachNumber { get; set; }
        public bool ShowProbabilityOfExpectedNumbers { get; set; }
        public bool ShowTop20MostLikely { get; set; }
        public bool ShowOnlyBetsAbove4Matches { get; set; }

        public int HowManyNumbers { get; set; }
    }

}
