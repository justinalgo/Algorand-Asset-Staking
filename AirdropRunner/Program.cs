using Airdrop;
using Algorand.V2;
using Util;
using System;
using System.Linq;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Account = Algorand.Account;
using Transaction = Algorand.Transaction;
using Algorand;
using Algorand.V2.Model;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Algorand.V2.Algod;
using Microsoft.Extensions.Logging;

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
                    services.AddTransient<IKeyManager, AirdropKey>();
                    services.AddTransient<IAlgoApi, AlgoApi>();
                    services.AddTransient<IAirdropFactory, AlchemonAirdropFactory>();
                    services.AddTransient<App>();
                });
    }
}
