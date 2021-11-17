using Airdrop.AirdropFactories.Holdings;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Util;
using Util.Cosmos;
using Util.KeyManagers;

[assembly: FunctionsStartup(typeof(AlchemonAirdropFunction.Startup))]

namespace AlchemonAirdropFunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddTransient<ICosmos, Cosmos>()
                .AddTransient<IAlgoApi, AlgoApi>()
                .AddTransient<IKeyManager, AirdropKey>()
                .AddTransient<IHoldingsAirdropFactory, AlchemonHoldingsFactory>();
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), optional: true, reloadOnChange: false);

            var settings = builder.ConfigurationBuilder.Build();

            string azureKeyVaultEndpoint = settings.GetValue<string>("Endpoints:AzureKeyVault");

            builder.ConfigurationBuilder
                .AddAzureKeyVault(azureKeyVaultEndpoint);
        }
    }
}
