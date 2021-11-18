using Xunit;
using Microsoft.Extensions.Configuration;

namespace AlchemonAirdropFunctionTest
{
    public class ConfigurationTest
    {
        [Fact]
        public void HoldingsAirdropScheduleTest()
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json");

            var settings = configurationBuilder.Build();

            Assert.Equal("0 0 16 * * Sat", settings["HoldingsAirdropSchedule"]);
        }

        [Fact]
        public void LiquidityAirdropScheduleTest()
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json");

            var settings = configurationBuilder.Build();

            Assert.Equal("0 30 16 * * Sat", settings["LiquidityAirdropSchedule"]);
        }
    }
}
