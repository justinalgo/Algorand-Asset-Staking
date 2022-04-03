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

            ulong alva = 553615859;
            ulong prepack = 465310574;
            ulong s1 = 557939659;

            var accounts = await indexerUtils.GetAccounts(alva);

            AirdropUnitCollectionManager manager = new AirdropUnitCollectionManager();

            foreach (var account in accounts)
            {
                foreach (var asset in account.Assets)
                {
                    if (asset.AssetId == prepack && asset.Amount > 0)
                    {
                        manager.AddAirdropUnit(new AirdropUnit(account.Address, alva, prepack, 9000, asset.Amount, true));
                    } else if (asset.AssetId == s1 && asset.Amount > 0)
                    {
                        manager.AddAirdropUnit(new AirdropUnit(account.Address, alva, s1, 9000, asset.Amount, true));
                    }
                }
            }

            IEnumerable<AirdropUnitCollection> collections = manager.GetAirdropUnitCollections();

            ulong total = 0;

            foreach (AirdropUnitCollection collection in collections.OrderByDescending(a => a.Total))
            {
                ulong? prepacks = collection.airdropUnits.FirstOrDefault(au => au.SourceAssetId == prepack)?.NumberOfSourceAsset;
                double modifier = 1;
                if (prepacks.HasValue)
                {
                    modifier += (double)(prepacks * .025);
                }
                ulong newTotal = (ulong) (collection.Total * modifier);
                total += newTotal;
                Console.WriteLine($"{collection.Wallet} : {newTotal}");
            }

            Console.WriteLine(total);
            Console.WriteLine(collections.Count());

            Console.ReadKey();

            List<SignedTransaction> signedTransactions = new List<SignedTransaction>();
            TransactionParametersResponse transactionParameters = await algodUtils.GetTransactionParams();

            foreach (AirdropUnitCollection collection in collections)
            {
                try
                {
                    Address address = new Address(collection.Wallet);

                    ulong? prepacks = collection.airdropUnits.FirstOrDefault(au => au.SourceAssetId == prepack)?.NumberOfSourceAsset;
                    double modifier = 1;
                    if (prepacks.HasValue)
                    {
                        modifier += (double)(prepacks * .025);
                    }
                    ulong newTotal = (ulong)(collection.Total * modifier);

                    Transaction txn = Transaction.CreateAssetTransferTransaction(
                            assetSender: key.GetAddress(),
                            assetReceiver: address,
                            assetCloseTo: null,
                            assetAmount: newTotal,
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
