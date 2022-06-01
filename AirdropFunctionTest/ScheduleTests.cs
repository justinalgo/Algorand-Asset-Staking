using AirdropFunction;
using Xunit;

namespace FunctionTest
{
    public class ScheduleTests
    {
        [Fact]
        public void RaptorHoldingsAirdropScheduleTest()
        {
            Assert.Equal("0 0 5 * * Mon", Function.RaptorHoldingsAirdropSchedule);
        }

        [Fact]
        public void CryptoBunnyHoldingsAirdropScheduleTest()
        {
            Assert.Equal("0 0 15 * * Mon", Function.CryptoBunnyHoldingsAirdropSchedule);
        }

        [Fact]
        public void NanaHoldingsAirdropScheduleTest()
        {
            Assert.Equal("0 0 14 * * Mon,Fri", Function.NanaHoldingsAirdropSchedule);
        }

        [Fact]
        public void ShrimpHoldingsAirdropScheduleTest()
        {
            Assert.Equal("0 0 16 * * Mon,Fri", Function.ShrimpHoldingsAirdropSchedule);
        }

        [Fact]
        public void AlchemonHoldingsAirdropScheduleTest()
        {
            Assert.Equal("0 0 16 * * Sat", Function.AlchemonHoldingsAirdropSchedule);
        }

        [Fact]
        public void AlchemonLiquidityAirdropScheduleTest()
        {
            Assert.Equal("0 0 16 * * Sun", Function.AlchemonLiquidityAirdropSchedule);
        }

        [Fact]
        public void MantisHoldingsAirdropScheduleTest()
        {
            Assert.Equal("0 0 18 * * *", Function.MantisHoldingsAirdropSchedule);
        }

        [Fact]
        public void AlvaHoldingsAirdropScheduleTest()
        {
            Assert.Equal("0 0 16 * * Sun,Tue,Thu,Sat", Function.MantisHoldingsAirdropSchedule);
        }
    }
}
