namespace LotteryCrawler
{
    public class RankingPositionInfo
    {
        public RankingPositionInfo(string summary = "", double weight = 0)
        {
            Summary = summary;
            Weight = weight;
        }

        public string Summary { get; set; }
        public double Weight { get; set; }
        public int Match1 { get; internal set; }
        public int Match2 { get; internal set; }
        public int Match3 { get; internal set; }
    }
}
