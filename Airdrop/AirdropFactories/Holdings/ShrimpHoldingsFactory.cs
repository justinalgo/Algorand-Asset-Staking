using Algorand.V2.Indexer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Util;
using Util.Cosmos;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Holdings
{
    public class ShrimpHoldingsFactory : HoldingsAirdropFactory
    {
        private readonly IIndexerUtils indexerUtils;
        private readonly ICosmos cosmos;
        private readonly HttpClient httpClient;

        public ShrimpHoldingsFactory(IIndexerUtils indexerUtils, ICosmos cosmos, IHttpClientFactory httpClientFactory)
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
            this.indexerUtils = indexerUtils;
            this.cosmos = cosmos;
            this.httpClient = httpClientFactory.CreateClient();
        }

        public override async Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts()
        {
            IDictionary<ulong, ulong> assetValues = await this.FetchAssetValues();
            ConcurrentBag<AirdropAmount> airdropAmounts = new ConcurrentBag<AirdropAmount>();
            IEnumerable<string> walletAddresses = await this.FetchWalletAddresses();
            IDictionary<string, ulong> ab2Values = await this.GetAb2Values(assetValues);
            IDictionary<string, ulong> randValues = await this.GetRandValues(assetValues);

            Parallel.ForEach<string>(walletAddresses, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async walletAddress =>
            {
                Account account = await this.indexerUtils.GetAccount(walletAddress);
                IEnumerable<AssetHolding> assetHoldings = account.Assets;
                ulong amount = this.GetAssetHoldingsAmount(assetHoldings, assetValues);

                amount += this.GetAb2Amount(walletAddress, ab2Values);
                amount += this.GetRandAmount(walletAddress, randValues);

                if (amount != 0)
                {
                    airdropAmounts.Add(new AirdropAmount(walletAddress, this.AssetId, amount));
                }
            });

            return airdropAmounts;
        }

        public override async Task<IEnumerable<string>> FetchWalletAddresses()
        {
            IEnumerable<string> walletAddresses = await this.indexerUtils.GetWalletAddresses(this.AssetId);

            return walletAddresses;
        }

        public override async Task<IDictionary<ulong, ulong>> FetchAssetValues()
        {
            IEnumerable<AssetValue> values = await cosmos.GetAssetValues("LingLing", "MNGO", "Yieldling");

            Dictionary<ulong, ulong> assetValues = values.ToDictionary(av => av.AssetId, av => av.Value);

            return assetValues;
        }

        public ulong GetAb2Amount(string walletAddress, IDictionary<string, ulong> ab2Values)
        {
            if (ab2Values.ContainsKey(walletAddress))
            {
                return ab2Values[walletAddress];
            }

            return 0;
        }

        private async Task<IDictionary<string, ulong>> GetAb2Values(IDictionary<ulong, ulong> assetValues)
        {
            Dictionary<string, ulong> walletValues = new Dictionary<string, ulong>();
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

        private ulong GetRandAmount(string walletAddress, IDictionary<string, ulong> randValues)
        {
            if (randValues.ContainsKey(walletAddress))
            {
                return randValues[walletAddress];
            }

            return 0;
        }

        public async Task<IDictionary<string, ulong>> GetRandValues(IDictionary<ulong, ulong> assetValues)
        {
            Dictionary<string, ulong> walletValues = new Dictionary<string, ulong>();
            IEnumerable<(ulong, string)> randSellers = await GetRandSellers();

            foreach ((ulong, string) randSeller in randSellers)
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

        private async Task<IEnumerable<(ulong, string)>> GetRandSellers()
        {
            List<(ulong, string)> randSellers = new List<(ulong, string)>();

            foreach (string creatorAddress in this.CreatorAddresses)
            {
                string randEndpoint = "https://www.randswap.com/v1/secondary/get-listings-for-creator?creatorAddress=" + creatorAddress;

                string jsonResponse = await httpClient.GetStringAsync(randEndpoint);
                Dictionary<string, dynamic> sellers = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonResponse);

                foreach (var assetId in sellers.Keys)
                {
                    if (assetId != "name" && assetId != "royalty" && assetId != "escrowAddress")
                    {
                        randSellers.Add((ulong.Parse(assetId), sellers[assetId]["seller"]));
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
        public ulong AssetId { get; set; }
    }
}
