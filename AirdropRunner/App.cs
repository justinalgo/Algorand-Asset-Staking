using Airdrop;
using Airdrop.AirdropFactories.Holdings;
using Airdrop.AirdropFactories.Liquidity;
using Algorand;
using Algorand.V2.Algod.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util;
using Util.Cosmos;
using Util.KeyManagers;
using Utils.Algod;
using Utils.Indexer;
using ApiException = Algorand.V2.Algod.Model.ApiException;
using Transaction = Algorand.Transaction;

namespace AirdropRunner
{
    public class App
    {
        private readonly ILogger<App> logger;
        private readonly IAlgodUtils algodUtils;
        private readonly IIndexerUtils indexerUtils;
        private readonly ICosmos cosmos;
        private readonly IKeyManager keyManager;
        private readonly IHoldingsAirdropFactory holdingsAirdropFactory;
        private readonly ILiquidityAirdropFactory liquidityAirdropFactory;

        public App(ILogger<App> logger, IAlgodUtils algodUtils, IIndexerUtils indexerUtils, ICosmos cosmos, IKeyManager keyManager, IHoldingsAirdropFactory holdingsAirdropFactory, ILiquidityAirdropFactory liquidityAirdropFactory)
        {
            this.logger = logger;
            this.algodUtils = algodUtils;
            this.indexerUtils = indexerUtils;
            this.cosmos = cosmos;
            this.keyManager = keyManager;
            this.holdingsAirdropFactory = holdingsAirdropFactory;
            this.liquidityAirdropFactory = liquidityAirdropFactory;
        }

        public async Task Run()
        {
            IEnumerable<AirdropAmount> amounts = await holdingsAirdropFactory.FetchAirdropAmounts();

            foreach (AirdropAmount amt in amounts.OrderByDescending(a => a.Amount))
            {
                Console.WriteLine($"{amt.Wallet} : {amt.Amount}");
            }

            Console.WriteLine(amounts.Sum(a => (double)a.Amount));
            Console.WriteLine(amounts.Count());
            
            Console.ReadKey();

            ulong lastRound = await algodUtils.GetLastRound();
            Console.WriteLine($"Round start: {lastRound}");

            Parallel.ForEach<AirdropAmount>(amounts, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async airdropAmount =>
            {
                try
                {
                    TransactionParametersResponse transactionParameters = await algodUtils.GetTransactionParams();

                    Address address = new Address(airdropAmount.Wallet);

                    Transaction txn = Transaction.CreateAssetTransferTransaction(
                            assetSender: keyManager.GetAddress(),
                            assetReceiver: address,
                            assetCloseTo: null,
                            assetAmount: airdropAmount.Amount,
                            flatFee: transactionParameters.Fee,
                            firstRound: transactionParameters.LastRound,
                            lastRound: transactionParameters.LastRound + 1000,
                            note: Encoding.UTF8.GetBytes(""),
                            genesisID: transactionParameters.GenesisId,
                            genesisHash: new Algorand.Digest(transactionParameters.GenesisHash),
                            assetIndex: airdropAmount.AssetId
                        );

                    SignedTransaction stxn = keyManager.SignTransaction(txn);

                    PostTransactionsResponse resp = await this.algodUtils.SubmitTransaction(stxn);
                    Console.WriteLine(airdropAmount.Wallet + " : " + airdropAmount.Amount + " with TxId: " + resp.TxId);
                }
                catch (ApiException<ErrorResponse> ex)
                {
                    Console.WriteLine(ex.Result.Message);
                    Console.WriteLine("ApiException on " + airdropAmount.Wallet);
                }
                catch (ArgumentException)
                {
                    Console.WriteLine(airdropAmount.Wallet + " is an invalid address");
                }
            });

            await this.algodUtils.GetStatusAfterRound(await this.algodUtils.GetLastRound() + 5);

            IEnumerable<string> walletAddresses = await this.indexerUtils.GetWalletAddresses(
                keyManager.GetAddress().EncodeAsString(),
                holdingsAirdropFactory.AssetId,
                addressRole: Algorand.V2.Indexer.Model.AddressRole.Sender,
                minRound: lastRound
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
