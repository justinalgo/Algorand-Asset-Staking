using Algorand.V2.Model;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Util;
using Util.Cosmos;

namespace Airdrop.AirdropFactories.Holdings
{
    public class ShrimpHoldingsFactory : IHoldingsAirdropFactory
    {
        public long AssetId { get; set; }
        public long Decimals { get; set; }
        public string[] CreatorAddresses { get; set; }
        private readonly IAlgoApi api;
        private readonly ICosmos cosmos;
        private readonly HttpClient httpClient;

        public ShrimpHoldingsFactory(IAlgoApi api, ICosmos cosmos, IHttpClientFactory httpClientFactory)
        {
            this.AssetId = 360019122;
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
            this.api = api;
            this.cosmos = cosmos;
            this.httpClient = httpClientFactory.CreateClient();
        }

        public async Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts()
        {
            IDictionary<long, long> assetValues = await this.FetchAssetValues();
            ConcurrentBag<AirdropAmount> airdropAmounts = new ConcurrentBag<AirdropAmount>();
            IEnumerable<string> walletAddresses = this.FetchWalletAddresses();
            IDictionary<string, long> ab2Values = await this.GetAb2Values(assetValues);
            IDictionary<string, long> randValues = await this.GetRandValues(assetValues);

            Parallel.ForEach<string>(walletAddresses, new ParallelOptions { MaxDegreeOfParallelism = 10 }, walletAddress =>
            {
                IEnumerable<AssetHolding> assetHoldings = this.api.GetAssetsByAddress(walletAddress);
                long amount = this.GetAssetHoldingsAmount(assetHoldings, assetValues);

                amount += this.GetAb2Amount(walletAddress, ab2Values);
                amount += this.GetRandAmount(walletAddress, randValues);

                if (amount != 0)
                {
                    airdropAmounts.Add(new AirdropAmount(walletAddress, this.AssetId, amount));
                }
            });

            return airdropAmounts;
        }

        public IEnumerable<string> FetchWalletAddresses()
        {
            IEnumerable<string> walletAddresses = this.api.GetWalletAddressesWithAsset(this.AssetId);

            return walletAddresses;
        }

        public async Task<IDictionary<long, long>> FetchAssetValues()
        {
            IEnumerable<AssetValue> values = await cosmos.GetAssetValues("LingLing", "MNGO", "Yieldling");

            Dictionary<long, long> assetValues = values.ToDictionary(av => av.AssetId, av => av.Value);

            return assetValues;
        }

        public long GetAssetHoldingsAmount(IEnumerable<AssetHolding> assetHoldings, IDictionary<long, long> assetValues)
        {
            long airdropAmount = 0;

            foreach (AssetHolding miniAssetHolding in assetHoldings)
            {
                if (miniAssetHolding.AssetId.HasValue &&
                    miniAssetHolding.Amount.HasValue &&
                    miniAssetHolding.Amount > 0 &&
                    assetValues.ContainsKey(miniAssetHolding.AssetId.Value))
                {
                    airdropAmount += (long)miniAssetHolding.Amount.Value * assetValues[miniAssetHolding.AssetId.Value];
                }
            }

            return airdropAmount;
        }

        public long GetAb2Amount(string walletAddress, IDictionary<string, long> ab2Values)
        {
            if (ab2Values.ContainsKey(walletAddress))
            {
                return ab2Values[walletAddress];
            }

            return 0;
        }

        private async Task<IDictionary<string, long>> GetAb2Values(IDictionary<long, long> assetValues)
        {
            Dictionary<string, long> walletValues = new Dictionary<string, long>();
            EscrowInfo escrowInfo = await GetAb2EscrowInfo();

            foreach (Escrow escrow in escrowInfo.MngoEscrows)
            {
                if (walletValues.ContainsKey(escrow.SellerWalletAddress) && assetValues.ContainsKey(escrow.AssetId))
                {
                    walletValues[escrow.SellerWalletAddress] += assetValues[escrow.AssetId];
                }
                else if (assetValues.ContainsKey(escrow.AssetId))
                {
                    walletValues[escrow.SellerWalletAddress] = assetValues[escrow.AssetId];
                }
            }

            foreach (Escrow escrow in escrowInfo.YieldingEscrows)
            {
                if (walletValues.ContainsKey(escrow.SellerWalletAddress) && assetValues.ContainsKey(escrow.AssetId))
                {
                    walletValues[escrow.SellerWalletAddress] += assetValues[escrow.AssetId];
                }
                else if (assetValues.ContainsKey(escrow.AssetId))
                {
                    walletValues[escrow.SellerWalletAddress] = assetValues[escrow.AssetId];
                }
            }

            foreach (Escrow escrow in escrowInfo.LingLingEscrows)
            {
                if (walletValues.ContainsKey(escrow.SellerWalletAddress) && assetValues.ContainsKey(escrow.AssetId))
                {
                    walletValues[escrow.SellerWalletAddress] += assetValues[escrow.AssetId];
                }
                else if (assetValues.ContainsKey(escrow.AssetId))
                {
                    walletValues[escrow.SellerWalletAddress] = assetValues[escrow.AssetId];
                }
            }

            return walletValues;
        }

        private async Task<EscrowInfo> GetAb2EscrowInfo()
        {
            string ab2endpoint = "https://linglingab2.vercel.app/api";

            string jsonResponse = await httpClient.GetStringAsync(ab2endpoint);
            EscrowInfo escrowInfo = JsonConvert.DeserializeObject<EscrowInfo>(jsonResponse);

            return escrowInfo;
        }

        private long GetRandAmount(string walletAddress, IDictionary<string, long> randValues)
        {
            if (randValues.ContainsKey(walletAddress))
            {
                return randValues[walletAddress];
            }

            return 0;
        }

        public async Task<IDictionary<string, long>> GetRandValues(IDictionary<long, long> assetValues)
        {
            Dictionary<string, long> walletValues = new Dictionary<string, long>();
            IEnumerable<(long, string)> randSellers = await GetRandSellers();

            foreach ((long, string) randSeller in randSellers)
            {
                if (walletValues.ContainsKey(randSeller.Item2) && assetValues.ContainsKey(randSeller.Item1))
                {
                    walletValues[randSeller.Item2] += assetValues[randSeller.Item1];
                }
                else if (assetValues.ContainsKey(randSeller.Item1))
                {
                    walletValues[randSeller.Item2] = assetValues[randSeller.Item1];
                }
            }

            return walletValues;
        }

        private async Task<IEnumerable<(long, string)>> GetRandSellers()
        {
            List<(long, string)> randSellers = new List<(long, string)>();

            foreach (string creatorAddress in this.CreatorAddresses)
            {
                string randEndpoint = "https://www.randswap.com/v1/secondary/get-listings-for-creator?creatorAddress=" + creatorAddress;

                string jsonResponse = await httpClient.GetStringAsync(randEndpoint);
                Dictionary<string, dynamic> sellers = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonResponse);

                foreach (var assetId in sellers.Keys)
                {
                    if (assetId != "name" && assetId != "royalty" && assetId != "escrowAddress")
                    {
                        randSellers.Add((long.Parse(assetId), sellers[assetId]["seller"]));
                    }
                }
            }

            return randSellers;
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
        public long AssetId { get; set; }
    }
}
