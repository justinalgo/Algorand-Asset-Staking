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
        static void Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();

            host.Services.GetService<App>().Run();

            /*AirdropFactory airdropFactory = new AlchemonAirdropFactory(api);

            IDictionary<long, long> assetValues = airdropFactory.GetAssetValues();

            IEnumerable<AirdropAmount> airdropAmounts = airdropFactory.FetchAirdropAmounts(assetValues);

            foreach (AirdropAmount airdropAmount in airdropAmounts)
            {
                Console.WriteLine(airdropAmount.Wallet + " : " + airdropAmount.Amount);
                TransactionParametersResponse transactionParameters = algod.TransactionParams();

                Transaction txn = Utils.GetTransferAssetTransaction(
                        account.Address,
                        new Address(airdropAmount.Wallet),
                        airdropFactory.AssetId,
                        (ulong)airdropAmount.Amount,
                        transactionParameters
                    );
                SignedTransaction stxn = account.SignTransaction(txn);

                api.SubmitTransaction(stxn);
            }

            Console.WriteLine("Total: " + airdropAmounts.Sum(aa => aa.Amount));
            Console.WriteLine("Number of wallets: " + airdropAmounts.Count());*/
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
                    services.AddTransient<IApi, Api>();
                    services.AddTransient<IAirdropFactory, ShrimpAirdropFactory>();
                    services.AddTransient<App>();
                });
    }
}
