using System;

namespace LotteryCrawler.Core.Strategies.ReadEngines
{

    public interface IReadEngine
    {
        void Read(int[][] history, BetNumber[] betNumbers);
    }

}
