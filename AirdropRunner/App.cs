using Airdrop;
using Airdrop.AirdropFactories.Holdings;
using Airdrop.AirdropFactories.Liquidity;
using Airdrop.AirdropFactories.Random;
using Airdrop.AirdropFactories.Unique;
using Airdrop.AirdropFactories.AcornPartners;
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
            Key key = keyManager.CavernaWallet;

            ulong total = (6 * 100) + (90) + (2 * 75) + (60) + (11 * 50);

            double ratio = 1000000 / (double) total;

            ulong weight50 = (ulong)(50 * ratio);
            ulong weight60 = (ulong)(60 * ratio);
            ulong weight75 = (ulong)(75 * ratio);
            ulong weight90 = (ulong)(90 * ratio);
            ulong weight100 = (ulong)(100 * ratio);

            var fact = new GoannaPartnerFactory(indexerUtils, weight100);

            var accounts = await fact.FetchAccounts();

            AirdropUnitCollectionManager manager = new AirdropUnitCollectionManager();

            //100algo
            await fact.FetchAirdropUnitCollections(manager, accounts);
            await new AlchemonPartnerFactory(indexerUtils, cosmos, weight100).FetchAirdropUnitCollections(manager, accounts);
            await new StarfacePartnerFactory(indexerUtils, weight100).FetchAirdropUnitCollections(manager, accounts);
            await new MngoPartnerFactory(indexerUtils, weight100).FetchAirdropUnitCollections(manager, accounts);
            await new BananaMintPartnerFactory(indexerUtils, weight100).FetchAirdropUnitCollections(manager, accounts);
            await new TrinleyPartnerFactory(indexerUtils, weight100).FetchAirdropUnitCollections(manager, accounts);

            //90algo
            await new ParlimentAowlPartnerFactory(indexerUtils, weight90).FetchAirdropUnitCollections(manager, accounts);

            //75algo
            await new AlgoGangPartnerFactory(indexerUtils, weight75).FetchAirdropUnitCollections(manager, accounts);
            await new SwappyPartnerFactory(indexerUtils, weight75).FetchAirdropUnitCollections(manager, accounts);

            //60algo
            await new AlgoOwlPartnerFactory(indexerUtils, weight60).FetchAirdropUnitCollections(manager, accounts);

            //50algo
            await new AlgoBotsPartnerFactory(indexerUtils, weight50).FetchAirdropUnitCollections(manager, accounts);
            await new AlgoPlanetsPartnerFactory(indexerUtils, weight50).FetchAirdropUnitCollections(manager, accounts);
            await new AlgoWhalesPartnerFactory(indexerUtils, weight50).FetchAirdropUnitCollections(manager, accounts);
            await new FlemishPartnerFactory(indexerUtils, weight50).FetchAirdropUnitCollections(manager, accounts);
            await new KnitHeadsPartnerFactory(indexerUtils, weight50).FetchAirdropUnitCollections(manager, accounts);
            await new LingLingPartnerFactory(indexerUtils, weight50).FetchAirdropUnitCollections(manager, accounts);
            await new MonstiPartnerFactory(indexerUtils, weight50).FetchAirdropUnitCollections(manager, accounts);
            await new MushiesPartnerFactory(indexerUtils, weight50).FetchAirdropUnitCollections(manager, accounts);
            await new StupidHorsePartnerFactory(indexerUtils, weight50).FetchAirdropUnitCollections(manager, accounts);
            await new TinyWhalesPartnerFactory(indexerUtils, weight50).FetchAirdropUnitCollections(manager, accounts);
            await new YieldlingPartnerFactory(indexerUtils, weight50).FetchAirdropUnitCollections(manager, accounts);

            IEnumerable<AirdropUnitCollection> collections = manager.GetAirdropUnitCollections();

            foreach (AirdropUnitCollection collection in collections.OrderByDescending(a => a.Total))
            {
                Console.WriteLine(collection.ToString());
            }

            Console.WriteLine(collections.Sum(c => (double)c.Total));
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
                            note: Encoding.UTF8.GetBytes(collection.ToString().Length < 1024 ? collection.ToString() : "Note too long..."),
                            //note: Encoding.UTF8.GetBytes(""),
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
            }
        }
    }
}
