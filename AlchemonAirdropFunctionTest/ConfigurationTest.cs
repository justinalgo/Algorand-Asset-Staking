using AlchemonAirdropFunction;
using Xunit;

namespace AlchemonAirdropFunctionTest
{
    public class ConfigurationTest
    {
        [Fact]
        public void HoldingsAirdropScheduleTest()
        {
            Assert.Equal("0 30 16 * * Sat", AlchemonAirdrop.HoldingsAirdropSchedule);
        }

        [Fact]
        public void LiquidityAirdropScheduleTest()
        {
            Assert.Equal("0 0 16 * * Sat", AlchemonAirdrop.LiquidityAirdropSchedule);
        }
    }
}
