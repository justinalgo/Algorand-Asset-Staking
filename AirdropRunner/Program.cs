using Airdrop.AirdropFactories.Holdings;
using Airdrop.AirdropFactories.Liquidity;
using Algorand.V2.Algod;
using Algorand.V2.Indexer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
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

                    services.AddHttpClient<IDefaultApi, DefaultApi>(client =>
                    {
                        client.BaseAddress = new Uri(context.Configuration["Endpoints:Algod"]);
                        client.DefaultRequestHeaders.Add("X-Algo-API-Token", context.Configuration["AlgodToken"]);
                        client.Timeout = Timeout.InfiniteTimeSpan;
                    });

                    services.AddHttpClient<ILookupApi, LookupApi>(client =>
                    {
                        client.BaseAddress = new Uri(context.Configuration["Endpoints:Indexer"]);
                        client.DefaultRequestHeaders.Add("X-Algo-API-Token", context.Configuration["IndexerToken"]);
                        client.Timeout = Timeout.InfiniteTimeSpan;
                    });

                    services.AddHttpClient<ISearchApi, SearchApi>(client =>
                    {
                        client.BaseAddress = new Uri(context.Configuration["Endpoints:Indexer"]);
                        client.DefaultRequestHeaders.Add("X-Algo-API-Token", context.Configuration["IndexerToken"]);
                        client.Timeout = Timeout.InfiniteTimeSpan;
                    });

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
