namespace LotteryCrawler.Bet.Strategies
{
    public interface IReadGames
    {
        BetNumber[] Read(int[][] numbers);
    }

}
