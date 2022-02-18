using Airdrop;
using Airdrop.AirdropFactories.Holdings;
using Algorand;
using Algorand.V2.Algod.Model;
using Microsoft.Azure.WebJobs;
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

namespace AirdropFunction
{
    public class Function
    {
        public const string NanaHoldingsAirdropSchedule = "0 0 14 * * Mon,Fri";
        public const string AlchemonLiquidityAirdropSchedule = "0 0 16 * * Sat";
        public const string RaptorHoldingsAirdropSchedule = "0 0 5 * * Mon";
        public const string ShrimpHoldingsAirdropSchedule = "0 0 16 * * Mon,Fri";
        private readonly IAlgodUtils algodUtils;
        private readonly IIndexerUtils indexerUtils;
        private readonly ICosmos cosmos;
        private readonly IKeyManager keyManager;
        private readonly IHttpClientFactory httpClientFactory;

        public Function(IAlgodUtils algodUtils, IIndexerUtils indexerUtils, ICosmos cosmos, IKeyManager keyManager, IHttpClientFactory httpClientFactory)
        {
            this.algodUtils = algodUtils;
            this.indexerUtils = indexerUtils;
            this.cosmos = cosmos;
            this.keyManager = keyManager;
            this.httpClientFactory = httpClientFactory;
        }

        [FunctionName("RaptorHoldingsAirdrop")]
        public async Task RaptorHoldingsAirdrop([TimerTrigger(RaptorHoldingsAirdropSchedule)] TimerInfo myTimer, ILogger log)
        {
            if (myTimer.IsPastDue)
            {
                log.LogError("Airdrop is past due. Erroring out...");
                throw new Exception("Airdrop past due");
            }

            Key CavernaKey = this.keyManager.CavernaWallet;
            RaptorHoldingsFactory factory = new RaptorHoldingsFactory(this.indexerUtils, this.cosmos, this.httpClientFactory);
            IEnumerable<AirdropUnitCollection> collections = await factory.FetchAirdropUnitCollections();

            foreach (AirdropUnitCollection collection in collections.OrderBy(c => c.Wallet))
            {
                log.LogInformation($"{collection.Wallet} : {collection.Total}");
            }

            log.LogInformation("Total drop: " + collections.Sum(a => (double)a.Total));
            log.LogInformation("Number of drops: " + collections.Count());

            List<SignedTransaction> signedTransactions = new List<SignedTransaction>();
            TransactionParametersResponse transactionParameters = await algodUtils.GetTransactionParams();

            foreach (AirdropUnitCollection collection in collections)
            {
                try
                {
                    Address address = new Address(collection.Wallet);

                    Transaction txn = Transaction.CreateAssetTransferTransaction(
                            assetSender: CavernaKey.GetAddress(),
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

                    SignedTransaction stxn = CavernaKey.SignTransaction(txn);

                    signedTransactions.Add(stxn);
                }
                catch (ArgumentException)
                {
                    log.LogWarning(collection.Wallet + " is an invalid address");
                }
            }

            ulong startingRound = await this.algodUtils.GetLastRound();
            log.LogInformation("Starting round: " + startingRound);

            await this.algodUtils.SubmitSignedTransactions(signedTransactions);

            await this.algodUtils.GetStatusAfterRound(await this.algodUtils.GetLastRound() + 5);

            var transactions = await indexerUtils.GetTransactions(CavernaKey.ToString(), addressRole: Algorand.V2.Indexer.Model.AddressRole.Sender, txType: Algorand.V2.Indexer.Model.TxType.Axfer, minRound: startingRound);
            HashSet<string> txIds = transactions.Select(t => t.Id).ToHashSet();

            foreach (SignedTransaction stxn in signedTransactions)
            {
                if (!txIds.Contains(stxn.transactionID))
                {
                    log.LogWarning("Failed to drop: " + stxn.tx.assetReceiver);
                }
            }
        }

        [FunctionName("NanaHoldingsAirdrop")]
        public async Task NanaHoldingsAirdrop([TimerTrigger(NanaHoldingsAirdropSchedule)] TimerInfo myTimer, ILogger log)
        {
            if (myTimer.IsPastDue)
            {
                log.LogError("Airdrop is past due. Erroring out...");
                throw new Exception("Airdrop past due");
            }

            Key CavernaKey = this.keyManager.CavernaWallet;
            NanaHoldingsFactory factory = new NanaHoldingsFactory(this.indexerUtils, this.cosmos, this.httpClientFactory);
            IEnumerable<AirdropUnitCollection> collections = await factory.FetchAirdropUnitCollections();

            foreach (AirdropUnitCollection collection in collections.OrderBy(c => c.Wallet))
            {
                log.LogInformation($"{collection.Wallet} : {collection.Total}");
            }

            log.LogInformation("Total drop: " + collections.Sum(a => (double)a.Total));
            log.LogInformation("Number of drops: " + collections.Count());

            List<SignedTransaction> signedTransactions = new List<SignedTransaction>();
            TransactionParametersResponse transactionParameters = await algodUtils.GetTransactionParams();

            foreach (AirdropUnitCollection collection in collections)
            {
                try
                {
                    Address address = new Address(collection.Wallet);

                    Transaction txn = Transaction.CreateAssetTransferTransaction(
                            assetSender: CavernaKey.GetAddress(),
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

                    SignedTransaction stxn = CavernaKey.SignTransaction(txn);

                    signedTransactions.Add(stxn);
                }
                catch (ArgumentException)
                {
                    log.LogWarning(collection.Wallet + " is an invalid address");
                }
            }

            ulong startingRound = await this.algodUtils.GetLastRound();
            log.LogInformation("Starting round: " + startingRound);

            await this.algodUtils.SubmitSignedTransactions(signedTransactions);

            await this.algodUtils.GetStatusAfterRound(await this.algodUtils.GetLastRound() + 5);

            var transactions = await indexerUtils.GetTransactions(CavernaKey.ToString(), addressRole: Algorand.V2.Indexer.Model.AddressRole.Sender, txType: Algorand.V2.Indexer.Model.TxType.Axfer, minRound: startingRound);
            HashSet<string> txIds = transactions.Select(t => t.Id).ToHashSet();

            foreach (SignedTransaction stxn in signedTransactions)
            {
                if (!txIds.Contains(stxn.transactionID))
                {
                    log.LogWarning("Failed to drop: " + stxn.tx.assetReceiver);
                }
            }
        }

        [FunctionName("ShrimpHoldingsAirdrop")]
        public async Task ShrimpHoldingsAirdrop([TimerTrigger(ShrimpHoldingsAirdropSchedule)] TimerInfo myTimer, ILogger log)
        {
            if (myTimer.IsPastDue)
            {
                log.LogError("Airdrop is past due. Erroring out...");
                throw new Exception("Airdrop past due");
            }

            Key shrimpKey = this.keyManager.ShrimpWallet;
            ShrimpHoldingsFactory factory = new ShrimpHoldingsFactory(this.indexerUtils, this.cosmos, this.httpClientFactory);
            IEnumerable<AirdropUnitCollection> collections = await factory.FetchAirdropUnitCollections();

            foreach (AirdropUnitCollection collection in collections.OrderBy(c => c.Wallet))
            {
                log.LogInformation($"{collection.Wallet} : {collection.Total}");
            }

            log.LogInformation("Total drop: " + collections.Sum(a => (double)a.Total));
            log.LogInformation("Number of drops: " + collections.Count());

            List<SignedTransaction> signedTransactions = new List<SignedTransaction>();
            TransactionParametersResponse transactionParameters = await algodUtils.GetTransactionParams();

            foreach (AirdropUnitCollection collection in collections)
            {
                try
                {
                    Address address = new Address(collection.Wallet);

                    Transaction txn = Transaction.CreateAssetTransferTransaction(
                            assetSender: shrimpKey.GetAddress(),
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

                    SignedTransaction stxn = shrimpKey.SignTransaction(txn);

                    signedTransactions.Add(stxn);
                }
                catch (ArgumentException)
                {
                    log.LogWarning(collection.Wallet + " is an invalid address");
                }
            }

            ulong startingRound = await this.algodUtils.GetLastRound();
            log.LogInformation("Starting round: " + startingRound);

            await this.algodUtils.SubmitSignedTransactions(signedTransactions);

            await this.algodUtils.GetStatusAfterRound(await this.algodUtils.GetLastRound() + 5);

            var transactions = await indexerUtils.GetTransactions(shrimpKey.ToString(), addressRole: Algorand.V2.Indexer.Model.AddressRole.Sender, txType: Algorand.V2.Indexer.Model.TxType.Axfer, minRound: startingRound);
            HashSet<string> txIds = transactions.Select(t => t.Id).ToHashSet();

            foreach (SignedTransaction stxn in signedTransactions)
            {
                if (!txIds.Contains(stxn.transactionID))
                {
                    log.LogWarning("Failed to drop: " + stxn.tx.assetReceiver);
                }
            }
        }

    }
}
