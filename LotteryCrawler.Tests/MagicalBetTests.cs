using LotteryCrawler.Bet.Model;
using Xunit;

namespace LotteryCrawler.Tests
{
    public class MagicalBetTests
    {
        [Fact]
        public void Validate_ToStringConstructor1()
        {
            var mb = new MagicalBet(new int[] { 1, 2, 3, 4, 5, 6 }, 1);
            Assert.Equal("Bet: 1-2-3-4-5-6.", mb.ToString());
        }

        [Fact]
        public void Validate_ToStringConstructor2()
        {
            var mb = new MagicalBet(new int[] { 1, 2, 3, 4, 5, 6 }, new int[] { 6,5,4,3,2,1 }, 1);
            Assert.Equal("Missing Numbers: 6-5-4-3-2-1.Bet: 1-2-3-4-5-6.", mb.ToString());
        }
    }
}