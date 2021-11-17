using Airdrop;
using Algorand;
using Algorand.Client;
using Algorand.V2.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Util;
using Transaction = Algorand.Transaction;
using System.IO;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Util.KeyManagers;
using Airdrop.AirdropFactories;

namespace AirdropRunner
{
    public class App
    {
        private readonly ILogger<App> logger;
        private readonly IAlgoApi api;
        private readonly IKeyManager keyManager;
        private readonly IAirdropFactory airdropFactory;

        public App(ILogger<App> logger, IAlgoApi api, IKeyManager keyManager, IAirdropFactory airdropFactory)
        {
            this.logger = logger;
            this.api = api;
            this.keyManager = keyManager;
            this.airdropFactory = airdropFactory;
        }

        public async Task Run()
        {
            var amounts = await airdropFactory.FetchAirdropAmounts();

            foreach (AirdropAmount amt in amounts.OrderByDescending(a => a.Amount))
            {
                Console.WriteLine($"{amt.Wallet} : {amt.Amount}");
            }

            Console.WriteLine(amounts.Sum(a => a.Amount));
            Console.WriteLine(amounts.Count());

            Console.ReadKey();

            long lastRound = api.GetLastRound().Value;
            Console.WriteLine($"Round start: {lastRound}");

            Parallel.ForEach<AirdropAmount>(amounts, airdropAmount =>
            {
                try
                {
                    TransactionParametersResponse transactionParameters = api.GetTransactionParams();

                    Address address = new Address(airdropAmount.Wallet);

                    Transaction txn = Utils.GetTransferAssetTransaction(
                            keyManager.GetAddress(),
                            address,
                            airdropAmount.AssetId,
                            (ulong)airdropAmount.Amount,
                            transactionParameters
                        );

                    SignedTransaction stxn = keyManager.SignTransaction(txn);

                    PostTransactionsResponse resp = api.SubmitTransaction(stxn);
                    Console.WriteLine(airdropAmount.Wallet + " : " + airdropAmount.Amount + " with TxId: " + resp.TxId);
                }
                catch (ApiException ex)
                {
                    Console.WriteLine(ex.ErrorCode);
                    Console.WriteLine(ex.ErrorContent);
                    Console.WriteLine("ApiException on " + airdropAmount.Wallet);
                }
                catch (ArgumentException)
                {
                    Console.WriteLine(airdropAmount.Wallet + " is an invalid address");
                }
            });

            api.GetStatusAfterRound(api.GetLastRound().Value + 5);

            IEnumerable<string> walletAddresses = api.GetAddressesSent(
                keyManager.GetAddress().EncodeAsString(),
                airdropFactory.AssetId,
                lastRound
            );

            if (amounts.Count() != walletAddresses.Count())
            {
                foreach (AirdropAmount amount in amounts)
                {
                    if (!walletAddresses.Contains(amount.Wallet))
                    {
                        Console.WriteLine($"Failed to drop: {amount.Wallet}");
                    }
                }
            }
            else
            {
                Console.WriteLine("All addresses dropped successfully!");
            }
        }
    }
}
