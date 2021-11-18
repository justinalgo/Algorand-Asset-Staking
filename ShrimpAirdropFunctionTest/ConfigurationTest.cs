using Microsoft.Extensions.Configuration;
using System;
using Xunit;

namespace ShrimpAirdropFunctionTest
{
    public class ConfigurationTest
    {
        [Fact]
        public void HoldingsAirdropScheduleTest()
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json");

            var settings = configurationBuilder.Build();

            Assert.Equal("0 0 16 * * Mon,Fri", settings["HoldingsAirdropSchedule"]);
        }
    }
}
