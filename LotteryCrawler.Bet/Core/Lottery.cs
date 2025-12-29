using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotteryCrawler.Bet.Core
{
    internal class Lottery : ILottery
    {
        public int[] GenerateBet(List<int> allNumbers, BetNumber[] allOptions, int maxNumOfElements)
        {
            var randomGenerator = new Random();
            var finalBid = new int[maxNumOfElements];
            try
            {
                for (var index = 0; index < maxNumOfElements; index++)
                {
                    if (allNumbers.Count > 0)
                    {

                        var randomPosition = randomGenerator.Next(0, allNumbers.Count - 1);
                        var num = allNumbers[randomPosition];
                        finalBid[index] = num;
                        UpdateBasedOnBet(num, ref allNumbers);
                    }
                    else
                        finalBid[index] = -1;
                }
            } catch { }
            return finalBid;
        }

        private int[] GenerateBet_Old(List<int> allNumbers, BetNumber[] allOptions)
        {
            var randomGenerator = new Random();
            var finalBid = new int[6];
            try
            {
                for (var index = 0; index < 6; index++)
                {
                    if (allNumbers.Count > 0)
                    {

                        var randomPosition = randomGenerator.Next(0, allNumbers.Count - 1);
                        var num = allNumbers[randomPosition];
                        finalBid[index] = num;
                        UpdateBasedOnBet(num, ref allNumbers);

                        foreach (var nuuum in allOptions[num - 1].UnlikelyNumber)
                        {
                            var count = allNumbers.Count(x => x == nuuum.Item1);
                            var numCountToRemove = Math.Floor(count * nuuum.Item2);
                            while (allNumbers.Any(x => x == nuuum.Item1) || numCountToRemove > 0)
                            {
                                allNumbers.Remove(nuuum.Item1);
                                numCountToRemove--;
                            }
                        }
                    }
                    else
                        finalBid[index] = -1;
                }
            } catch { }
            return finalBid;
        }

        protected void UpdateBasedOnBet(int num, ref List<int> allNumbers)
        {
            allNumbers.RemoveAll(a => a == num);
        }

        public List<int> CreateNumbersAndProbabilities(BetNumber[] allNumbers)
        {
            if (allNumbers == null || allNumbers.Length != 60)
                throw new ArgumentException("allNumbers not valid.");

            var maxProb = allNumbers.Max(a => a.PositiveProbability) ?? 10;
            var finalNumbers = new List<int>();
            foreach (var number in allNumbers)
            {
                var numOfRepeat = maxProb * (number.PositiveProbability ?? maxProb);
                if (numOfRepeat == 0)
                    finalNumbers.AddRange(Enumerable.Repeat(number.Number, 1));
                else if (numOfRepeat > 0)
                    finalNumbers.AddRange(Enumerable.Repeat(number.Number, (int)Math.Floor(numOfRepeat * 100)));
            }
            return finalNumbers;
        }
    }
}
