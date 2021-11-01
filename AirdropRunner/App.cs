using Airdrop;
using Algorand;
using Algorand.V2.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Util;
using Transaction = Algorand.Transaction;

namespace AirdropRunner
{
    public class App
    {
        private readonly ILogger<App> logger;
        private readonly IApi api;
        private readonly IKeyManager keyManager;
        private readonly IAirdropFactory airdropFactory;

        public App(ILogger<App> logger, IApi api, IKeyManager keyManager, IAirdropFactory airdropFactory)
        {
            this.logger = logger;
            this.api = api;
            this.keyManager = keyManager;
            this.airdropFactory = airdropFactory;
        }

        public void Run()
        {
            var values = airdropFactory.GetAssetValues();
            var amounts = airdropFactory.FetchAirdropAmounts(values);

            Console.WriteLine(amounts.Sum(a => a.Amount));
            Console.WriteLine(amounts.Count());

            /*Parallel.ForEach<AirdropAmount>(amounts, airdropAmount =>
            {
                TransactionParametersResponse transactionParameters = api.GetTransactionParams();

                Transaction txn = Utils.GetTransferAssetTransaction(
                        keyManager.GetAddress(),
                        new Address(airdropAmount.Wallet),
                        airdropFactory.AssetId,
                        (ulong)airdropAmount.Amount,
                        transactionParameters
                    );

                SignedTransaction stxn = keyManager.SignTransaction(txn);

                PendingTransactionResponse resp = api.SubmitTransaction(stxn);
                Console.WriteLine(airdropAmount.Wallet + " : " + airdropAmount.Amount + " in round " + resp.ConfirmedRound);
            });*/
        }
    }
}
