using NanaAirdropFunction;
using Xunit;

namespace NanaAirdropFunctionTest
{
    public class ConfigurationTest
    {
        [Fact]
        public void HoldingsAirdropScheduleTest()
        {
            Assert.Equal("0 0 14 * * Mon,Fri", NanaAirdrop.HoldingsAirdropSchedule);
        }
    }
}
