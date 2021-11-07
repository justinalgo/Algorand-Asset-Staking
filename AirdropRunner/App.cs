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
using AssetValue = Util.AssetValue;
using System.IO;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

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
            var values = await airdropFactory.GetAssetValues();
            var amounts = airdropFactory.FetchAirdropAmounts(values);

            Console.WriteLine(amounts.Sum(a => a.Amount));
            Console.WriteLine(amounts.Count());
            Console.ReadKey();

            Parallel.ForEach<AirdropAmount>(amounts, airdropAmount =>
            {
                try
                {
                    TransactionParametersResponse transactionParameters = api.GetTransactionParams();

                    Address address = new Address(airdropAmount.Wallet);

                    Transaction txn = Utils.GetTransferAssetTransaction(
                            keyManager.GetAddress(),
                            address,
                            airdropFactory.AssetId,
                            (ulong)airdropAmount.Amount,
                            transactionParameters
                        );

                    SignedTransaction stxn = keyManager.SignTransaction(txn);

                    PendingTransactionResponse resp = api.SubmitTransaction(stxn);
                    Console.WriteLine(airdropAmount.Wallet + " : " + airdropAmount.Amount + " in round " + resp.ConfirmedRound);
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
        }
    }
}
