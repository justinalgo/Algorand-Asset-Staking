using Algorand.V2.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Util;

namespace Airdrop
{
    public class ShrimpAirdropFactory : AirdropFactory
    {
        private readonly IApi _api;
        private readonly HttpClient _client;

        public ShrimpAirdropFactory(IApi api) : base(360019122) {
            this._api = api;
            this._client = new HttpClient();
        }

        public override IEnumerable<AirdropAmount> FetchAirdropAmounts(IDictionary<long, long> assetValues)
        {
            List<AirdropAmount> airdropAmounts = new List<AirdropAmount>();
            IEnumerable<string> walletAddresses = this.FetchWalletAddresses();
            IDictionary<string, long> ab2Values = this.GetAb2Values(assetValues);

            foreach (string walletAddress in walletAddresses)
            {
                IEnumerable<AssetHolding> assetHoldings = this._api.GetAssetsByAddress(walletAddress);
                long amount = this.GetAssetHoldingsAmount(assetHoldings, assetValues);

                amount += this.GetAb2Amount(walletAddress, ab2Values);

                if (amount != 0)
                {
                    airdropAmounts.Add(new AirdropAmount(walletAddress, amount));
                }
            }

            return airdropAmounts;
        }

        public override IEnumerable<string> FetchWalletAddresses()
        {
            IEnumerable<string> walletAddresses = this._api.GetWalletAddressesWithAsset(this.AssetId);

            return walletAddresses;
        }

        public override IDictionary<long, long> GetAssetValues()
        {
            List<AssetValue> values = JsonSerializer.Deserialize<List<AssetValue>>(File.ReadAllText("C:/Users/ParkG/source/repos/Airdrop/Airdrop/ShrimpValues.json"));

            Dictionary<long, long> assetValues = values.ToDictionary(av => av.AssetId, av => av.Value);

            return assetValues;
        }

        public override long GetAssetHoldingsAmount(IEnumerable<AssetHolding> assetHoldings, IDictionary<long, long> assetValues)
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

        private IDictionary<string, long> GetAb2Values(IDictionary<long, long> assetValues)
        {
            Dictionary<string, long> walletValues = new Dictionary<string, long>();
            EscrowInfo escrowInfo = GetAb2EscrowInfo().Result;

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

            EscrowInfo escrowInfo = await this._client.GetFromJsonAsync<EscrowInfo>(ab2endpoint);
            
            return escrowInfo;
        }
    }

    class EscrowInfo
    {
        [JsonPropertyName("lingLingAb2Escrows")]
        public List<Escrow> LingLingEscrows { get; set; }
        [JsonPropertyName("mngosAb2Escrows")]
        public List<Escrow> MngoEscrows { get; set; }
        [JsonPropertyName("yieldlingsAb2Escrows")]
        public List<Escrow> YieldingEscrows { get; set; }
    }

    class Escrow
    {
        [JsonPropertyName("escrow")]
        public string EscrowWalletAddress { get; set; }
        [JsonPropertyName("seller")]
        public string SellerWalletAddress { get; set; }
        [JsonPropertyName("asset")]
        public string UnitName { get; set; }
        [JsonPropertyName("assetId")]
        public long AssetId { get; set; }
    }
}
