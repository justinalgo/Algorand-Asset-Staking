using Util;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Util.KeyManagers;
using Util.Cosmos;
using Airdrop.AirdropFactories.Holdings;

namespace AirdropRunner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();

            await host.Services.GetService<App>().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    var settings = config.Build();

                    string azureKeyVaultEndpoint = settings.GetValue<string>("Endpoints:AzureKeyVault");

                    config.AddAzureKeyVault(azureKeyVaultEndpoint);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging(configure => configure.AddConsole());
                    services.AddTransient<ICosmos, Cosmos>();
                    services.AddTransient<IAlgoApi, AlgoApi>();
                    services.AddTransient<IKeyManager, AirdropKey>();
                    services.AddTransient<IHoldingsAirdropFactory, AlchemonHoldingsFactory>();
                    services.AddTransient<App>();
                });
    }
}
