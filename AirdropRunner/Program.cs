﻿using Airdrop.AirdropFactories.Holdings;
using Airdrop.AirdropFactories.Liquidity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Util;
using Util.Cosmos;
using Util.KeyManagers;
using Utils.Algod;
using Utils.Indexer;

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
                    services.AddHttpClient();
                    services.AddTransient<IAlgodUtils, AlgodUtils>();
                    services.AddTransient<IIndexerUtils, IndexerUtils>();
                    services.AddTransient<ICosmos, Cosmos>();
                    services.AddTransient<IKeyManager, AirdropKey>();
                    services.AddTransient<IHoldingsAirdropFactory, AlchemonHoldingsFactory>();
                    services.AddTransient<ILiquidityAirdropFactory, AlchemonLiquidityFactory>();
                    services.AddTransient<App>();
                });
    }
}
