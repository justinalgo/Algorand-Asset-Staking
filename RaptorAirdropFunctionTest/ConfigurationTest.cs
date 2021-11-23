using RaptorAirdropFunction;
using System;
using Xunit;

namespace NanaAirdropFunctionTest
{
    public class ConfigurationTest
    {
        [Fact]
        public void HoldingsAirdropScheduleTest()
        {
            Assert.Equal("0 0 5 * * Mon", RaptorAirdrop.HoldingsAirdropSchedule);
        }
    }
}
