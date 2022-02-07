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
using Encoder = Algorand.Encoder;
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
            IEnumerable<AirdropUnitCollection> collections = await holdingsAirdropFactory.FetchAirdropUnitCollections();
            
            foreach (AirdropUnitCollection collection in collections.OrderByDescending(a => a.Total))
            {
                Console.WriteLine($"{collection.Wallet} : {collection.Total}");
            }

            Console.WriteLine(collections.Sum(a => (double)a.Total));
            Console.WriteLine(collections.Count());
            
            Console.ReadKey();

            ulong lastRound = await algodUtils.GetLastRound();
            Console.WriteLine($"Round start: {lastRound}");

            foreach (AirdropUnitCollection collection in collections)
            {
                try
                {
                    TransactionParametersResponse transactionParameters = await algodUtils.GetTransactionParams();

                    Address address = new Address(collection.Wallet);

                    Transaction txn = Transaction.CreateAssetTransferTransaction(
                            assetSender: keyManager.GetAddress(),
                            assetReceiver: address,
                            assetCloseTo: null,
                            assetAmount: collection.Total,
                            flatFee: transactionParameters.Fee,
                            firstRound: transactionParameters.LastRound,
                            lastRound: transactionParameters.LastRound + 1000,
                            note: Encoding.UTF8.GetBytes(""),
                            genesisID: transactionParameters.GenesisId,
                            genesisHash: new Algorand.Digest(transactionParameters.GenesisHash),
                            assetIndex: collection.DropAssetId
                        );

                    SignedTransaction stxn = keyManager.SignTransaction(txn);

                    PostTransactionsResponse resp = await this.algodUtils.SubmitTransaction(stxn);
                    Console.WriteLine(collection.Wallet + " : " + collection.Total + " with TxId: " + resp.TxId);
                }
                catch (ApiException<ErrorResponse> ex)
                {
                    Console.WriteLine(ex.Result.Message);
                    Console.WriteLine("ApiException on " + collection.Wallet);
                }
                catch (ArgumentException)
                {
                    Console.WriteLine(collection.Wallet + " is an invalid address");
                }
                catch (AggregateException ex)
                {
                    Console.WriteLine(ex.Flatten().Message);
                }
            }

            await this.algodUtils.GetStatusAfterRound(await this.algodUtils.GetLastRound() + 5);

            IEnumerable<string> walletAddresses = await this.indexerUtils.GetWalletAddresses(
                keyManager.GetAddress().EncodeAsString(),
                holdingsAirdropFactory.DropAssetId,
                addressRole: Algorand.V2.Indexer.Model.AddressRole.Sender,
                minRound: lastRound
            );

            if (collections.Count() != walletAddresses.Count())
            {
                foreach (AirdropUnitCollection collection in collections)
                {
                    if (!walletAddresses.Contains(collection.Wallet))
                    {
                        Console.WriteLine($"Failed to drop: {collection.Wallet}");
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
