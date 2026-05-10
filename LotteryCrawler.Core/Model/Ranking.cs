using LotteryCrawler.Core;
using LotteryCrawler.Core.Display;

namespace LotteryCrawler
{
    public class Ranking
    {
        private static Dictionary<string, RankingPositionInfo> _topAlgorithms = new Dictionary<string, RankingPositionInfo>();
        private static Dictionary<string, RankingPositionInfo> _topWorstAlgorithms = new Dictionary<string, RankingPositionInfo>();
        private static IList<Card> _valuableAlgorithms = new List<Card>();
        private static IList<RankingPositionInfo2> _summary1 = new List<RankingPositionInfo2>();
        private static IList<RankingPositionInfo3> _summary2 = new List<RankingPositionInfo3>();


        public void SummarizePerformance(IEnumerable<Card> cardsInfo, ILotteryEngine engine)
        {
            if (cardsInfo.Any())
            {
                var info = cardsInfo.Select(c => c.ResultGame.Sum(x => x.Number));
                _valuableAlgorithms = cardsInfo.Where(engine.ValuableResult()).OrderByDescending(x => x.MatchedNumbers.Count()).ToList();

                _summary1 = _valuableAlgorithms.GroupBy(x => x.EngineName)
                                .OrderByDescending(X => X.Max(X => X.MatchedNumbers.Count()))
                                .ThenByDescending(x => x.Count())
                                .Select(x => new RankingPositionInfo2
                                {
                                    Summary = x.Key,
                                    MaxMatchCount = x.Max(a => a.MatchedNumbers.Count()),
                                    ManyEffectiveDraws = x.Count()
                                }).ToList();

                _summary2 = _valuableAlgorithms.GroupBy(x => new { Engine = x.EngineName, MatchCount = x.MatchedNumbers.Count(), MissedNumbers = x.ResultGame.Select(x => x.Number).Except(x.MatchedNumbers).ToList() })
                                .OrderByDescending(g => g.Key.MatchCount)
                                .ThenByDescending(x => x.Sum(z => z.MatchedNumbers.Count()))
                                .Select(x => new RankingPositionInfo3
                                {
                                    Summary = x.Key.Engine,
                                    MatchCount = x.Key.MatchCount,
                                    EffectiveDrawsHaving = x.Count(),
                                    MissedNumbers = string.Join(" - ", x.Key.MissedNumbers)
                                }).ToList();

                var classifyingAlgorithms = _valuableAlgorithms.GroupBy(x => x.EngineName)
                                .Select(k => new RankingPositionInfo
                                {
                                    Summary = k.Key,
                                    Weight = k.Where(engine.VerifyMatch1()).Sum(a => a.MatchedNumbers.Count() * 300)
                                            + k.Where(engine.VerifyMatch2()).Sum(a => a.MatchedNumbers.Count() * 33)
                                            + k.Where(engine.VerifyMatch3()).Sum(a => a.MatchedNumbers.Count()),
                                    Match1 = k.Count(engine.VerifyMatch1()),
                                    Match2 = k.Count(engine.VerifyMatch2()),
                                    Match3 = k.Count(engine.VerifyMatch3()) 
                                });


                foreach (var item in classifyingAlgorithms)
                {
                    if (_topAlgorithms.ContainsKey(item.Summary))
                    {
                        _topAlgorithms[item.Summary].Weight += item.Weight;
                        _topAlgorithms[item.Summary].Match1 += item.Match1;
                        _topAlgorithms[item.Summary].Match2 += item.Match2;
                        _topAlgorithms[item.Summary].Match3 += item.Match3;
                    }
                    else
                        _topAlgorithms.Add(item.Summary, item);
                }
            }
        }

        public IList<RankingPositionInfo2> Summary1 { get { return _summary1; } }
        public IList<RankingPositionInfo3> Summary2 { get { return _summary2; } }

        public void Output()
        {
            var top = _topAlgorithms.OrderByDescending(x => x.Value.Weight).ToList();
            OutputFormatter.PrintLineSeparator();
            Printout("Top best 10 algorithms 6 * 300 |  5 * 33 | 4 * 1", top.Take(10));

            OutputFormatter.PrintLineSeparator();
            Printout("Top 5 worst algorithms:", top.TakeLast(10));
        }

        private static void Printout(string title, IEnumerable<KeyValuePair<string, RankingPositionInfo>> classifyingAlgorithms)
        {
            OutputFormatter.PrintSectionTitle(title);
            foreach (var item in classifyingAlgorithms)
            {
                Console.WriteLine(string.Format("{0,-20} | {1,-20} | Match1: {2, -5} | Match2: {3, -5} | Match3: {4, -5} | Total Matches: {5, -5}"
                                                , item.Key, item.Value.Summary, item.Value.Match1, item.Value.Match2, item.Value.Match3, item.Value.Match1 + item.Value.Match2 + item.Value.Match3));
            }
        }

    }
}
