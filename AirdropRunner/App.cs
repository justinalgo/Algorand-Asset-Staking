﻿using Airdrop;
using Airdrop.AirdropFactories.Holdings;
using Algorand;
using Algorand.V2.Algod.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Cosmos;
using Utils.Indexer;
using Utils.KeyManagers;
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
        private readonly IHttpClientFactory httpClientFactory;

        public App(ILogger<App> logger, IAlgodUtils algodUtils, IIndexerUtils indexerUtils, ICosmos cosmos, IKeyManager keyManager, IHttpClientFactory httpClientFactory)
        {
            this.logger = logger;
            this.algodUtils = algodUtils;
            this.indexerUtils = indexerUtils;
            this.cosmos = cosmos;
            this.keyManager = keyManager;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task Run()
        {
            ulong round = 19321702;
            Key shrimpKey = this.keyManager.ShrimpWallet;

            var transactions = await indexerUtils.GetTransactions(shrimpKey.ToString(), addressRole: Algorand.V2.Indexer.Model.AddressRole.Sender, txType: Algorand.V2.Indexer.Model.TxType.Axfer, minRound: round);
            HashSet<string> txIds = transactions.Select(t => t.Id).ToHashSet();

            Console.WriteLine(transactions.Count());

            foreach (var txn in transactions)
            {
                if (!txIds.Contains(txn.Id))
                {
                    Console.WriteLine("Failed to drop: " + txn.AssetTransferTransaction.Receiver);
                }
            }

            /*Key key = keyManager.CavernaWallet;
            var factory = new RaptorHoldingsFactory(indexerUtils, cosmos, httpClientFactory);

            IEnumerable<AirdropUnitCollection> collections = await factory.FetchAirdropUnitCollections();

            foreach (AirdropUnitCollection collection in collections.OrderByDescending(a => a.Total))
            {
                Console.WriteLine($"{collection.Wallet} : {collection.DropAssetId} : {collection.Total}");
            }

            Console.WriteLine(collections.Sum(a => (double)a.Total));
            Console.WriteLine(collections.Count());

            Console.ReadKey();

            List<SignedTransaction> signedTransactions = new List<SignedTransaction>();
            TransactionParametersResponse transactionParameters = await algodUtils.GetTransactionParams();

            foreach (AirdropUnitCollection collection in collections)
            {
                try
                {
                    Address address = new Address(collection.Wallet);

                    Transaction txn = Transaction.CreateAssetTransferTransaction(
                            assetSender: key.GetAddress(),
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

                    SignedTransaction stxn = key.SignTransaction(txn);

                    signedTransactions.Add(stxn);
                }
                catch (ArgumentException)
                {
                    Console.WriteLine(collection.Wallet + " is an invalid address");
                }
            }

            ulong startingRound = await this.algodUtils.GetLastRound();
            Console.WriteLine("Starting round: " + startingRound);

            await this.algodUtils.SubmitSignedTransactions(signedTransactions);

            await this.algodUtils.GetStatusAfterRound(await this.algodUtils.GetLastRound() + 5);

            var transactions = await indexerUtils.GetTransactions(key.ToString(), addressRole: Algorand.V2.Indexer.Model.AddressRole.Sender, txType: Algorand.V2.Indexer.Model.TxType.Axfer, minRound: startingRound);
            HashSet<string> txIds = transactions.Select(t => t.Id).ToHashSet();

            foreach (SignedTransaction stxn in signedTransactions)
            {
                if (!txIds.Contains(stxn.transactionID))
                {
                    Console.WriteLine("Failed to drop: " + stxn.tx.assetReceiver);
                }
            }*/
        }
    }
}
