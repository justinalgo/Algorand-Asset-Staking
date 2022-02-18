using AirdropFunction;
using System;
using Xunit;

namespace FunctionTest
{
    public class ScheduleTests
    {
        [Fact]
        public void RaptorHoldingsFactoryScheduleTest()
        {
            Assert.Equal("0 0 5 * * Mon", Function.RaptorHoldingsAirdropSchedule);
        }

        [Fact]
        public void AlchemonLiquidityAirdropScheduleTest()
        {
            Assert.Equal("0 0 16 * * Sat", Function.AlchemonLiquidityAirdropSchedule);
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
    }
}
