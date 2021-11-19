using ShrimpAirdropFunction;
using System;
using Xunit;

namespace ShrimpAirdropFunctionTest
{
    public class ConfigurationTest
    {
        [Fact]
        public void HoldingsAirdropScheduleTest()
        {
            Assert.Equal("0 0 16 * * Mon,Fri", ShrimpAirdrop.HoldingsAirdropSchedule);
        }
    }
}
