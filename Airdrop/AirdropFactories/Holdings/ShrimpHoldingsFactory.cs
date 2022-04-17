using Algorand.V2.Indexer.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Cosmos;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Holdings
{
    public class ShrimpHoldingsFactory : RandHoldingsAirdropFactory
    {
        private readonly ICosmos cosmos;

        public ShrimpHoldingsFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ICosmos cosmos, IHttpClientFactory httpClientFactory) : base(indexerUtils, algodUtils, httpClientFactory.CreateClient())
        {
            this.DropAssetId = 360019122;
            this.Decimals = 0;
            this.CreatorAddresses = new string[]
            {
                "TIMPJ6P5FZRNNKYJLAYD44XFOSUWEOUAR6NRWJMQR66BRM3QH7UUWEHA24",
                "MNGOLDXO723TDRM6527G7OZ2N7JLNGCIH6U2R4MOCPPLONE3ZATOBN7OQM",
                "MNGORTG4A3SLQXVRICQXOSGQ7CPXUPMHZT3FJZBIZHRYAQCYMEW6VORBIA",
                "MNGOZ3JAS3C4QTGDQ5NVABUEZIIF4GAZY52L3EZE7BQIBFTZCNLQPXHRHE",
                "MNGO4JTLBN64PJLWTQZYHDMF2UBHGJGW5L7TXDVTJV7JGVD5AE4Y3HTEZM",
                "5DYIZMX7N4SAB44HLVRUGLYBPSN4UMPDZVTX7V73AIRMJQA3LKTENTLFZ4",
            };
            this.cosmos = cosmos;
        }

        public override async Task<IDictionary<ulong, ulong>> FetchAssetValues()
        {
            IEnumerable<AssetValue> values = await cosmos.GetAssetValues("LingLing", "MNGO", "Yieldling");

            Dictionary<ulong, ulong> assetValues = values.ToDictionary(av => av.AssetId, av => av.Value);

            return assetValues;
        }

        public override async Task<IEnumerable<AirdropUnitCollection>> FetchAirdropUnitCollections()
        {
            IDictionary<ulong, ulong> assetValues = await FetchAssetValues();
            var accounts = await FetchAccounts();
            IDictionary<string, List<(ulong, ulong)>> randAccounts = await FetchRandAccounts();
            IDictionary<string, List<(ulong, ulong)>> ab2Accounts = await FetchAb2Accounts();

            AirdropUnitCollectionManager collectionManager = new AirdropUnitCollectionManager();

            Parallel.ForEach(accounts, new ParallelOptions { MaxDegreeOfParallelism = 10 }, account =>
            {
                AddAssetsInAccount(collectionManager, account, assetValues);

                string address = account.Address;

                if (randAccounts.ContainsKey(address))
                {
                    AddAssetsInList(collectionManager, address, randAccounts[address], assetValues);
                }

                if (ab2Accounts.ContainsKey(address))
                {
                    this.AddAssetsInList(collectionManager, address, ab2Accounts[address], assetValues);
                }
            });

            return collectionManager.GetAirdropUnitCollections();
        }

        public async Task<Dictionary<string, List<(ulong, ulong)>>> FetchAb2Accounts()
        {
            Dictionary<string, List<(ulong, ulong)>> ab2Accounts = new Dictionary<string, List<(ulong, ulong)>>();
            EscrowInfo escrowInfo = await GetAb2EscrowInfo();

            foreach (Escrow escrow in escrowInfo.LingLingEscrows)
            {
                ulong assetId = escrow.AssetId;
                string address = escrow.SellerWalletAddress;

                if (ab2Accounts.ContainsKey(address))
                {
                    ab2Accounts[address].Add((assetId, 1));
                }
                else
                {
                    ab2Accounts[address] = new List<(ulong, ulong)>() { (assetId, 1) };
                }
            }

            foreach (Escrow escrow in escrowInfo.MngoEscrows)
            {
                ulong assetId = escrow.AssetId;
                string address = escrow.SellerWalletAddress;

                if (ab2Accounts.ContainsKey(address))
                {
                    ab2Accounts[address].Add((assetId, 1));
                }
                else
                {
                    ab2Accounts[address] = new List<(ulong, ulong)>() { (assetId, 1) };
                }
            }

            foreach (Escrow escrow in escrowInfo.YieldingEscrows)
            {
                ulong assetId = escrow.AssetId;
                string address = escrow.SellerWalletAddress;

                if (ab2Accounts.ContainsKey(address))
                {
                    ab2Accounts[address].Add((assetId, 1));
                }
                else
                {
                    ab2Accounts[address] = new List<(ulong, ulong)>() { (assetId, 1) };
                }
            }

            return ab2Accounts;
        }

        private async Task<EscrowInfo> GetAb2EscrowInfo()
        {
            string ab2endpoint = "https://linglingab2.vercel.app/api";

            string jsonResponse = await this.HttpClient.GetStringAsync(ab2endpoint);
            EscrowInfo escrowInfo = JsonConvert.DeserializeObject<EscrowInfo>(jsonResponse);

            return escrowInfo;
        }
    }

    class EscrowInfo
    {
        [JsonProperty("lingLingAb2Escrows")]
        public List<Escrow> LingLingEscrows { get; set; }
        [JsonProperty("mngosAb2Escrows")]
        public List<Escrow> MngoEscrows { get; set; }
        [JsonProperty("yieldlingsAb2Escrows")]
        public List<Escrow> YieldingEscrows { get; set; }
    }

    class Escrow
    {
        [JsonProperty("escrow")]
        public string EscrowWalletAddress { get; set; }
        [JsonProperty("seller")]
        public string SellerWalletAddress { get; set; }
        [JsonProperty("asset")]
        public string UnitName { get; set; }
        [JsonProperty("assetId")]
        public ulong AssetId { get; set; }
    }
}
