using Algorand.V2.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Util;

namespace Airdrop
{
    public class AlchemonAirdropFactory : AirdropFactory
    {
        private readonly IApi _api;
        private readonly long _stakeFlagAssetId;

        public AlchemonAirdropFactory(IApi api) : base(310014962) {
            this._api = api;
            this._stakeFlagAssetId = 320570576;
        }

        public override IDictionary<long, long> GetAssetValues()
        {
            List<AlcheCoinAsset> alchemonValues = JsonConvert.DeserializeObject<List<AlcheCoinAsset>>(File.ReadAllText("C:/Users/ParkG/source/repos/Airdrop/AlcheCoinAirdrop/AlchemonValues.json"));

            Dictionary<long, long> assetValues = alchemonValues.ToDictionary(av => av.AssetId, av => av.Value);

            return assetValues;
        }

        public override IEnumerable<string> FetchWalletAddresses()
        {
            IEnumerable<string> walletAddresses = this._api.GetWalletAddressesWithAsset(this.AssetId, this._stakeFlagAssetId);

            return walletAddresses;
        }

        public override IEnumerable<AirdropAmount> FetchAirdropAmounts(IDictionary<long, long> assetValues)
        {
            List<AirdropAmount> airdropAmounts = new List<AirdropAmount>();
            IEnumerable<string> walletAddresses = this.FetchWalletAddresses();

            foreach (string walletAddress in walletAddresses)
            {
                IEnumerable<AssetHolding> assetHoldings = this._api.GetAssetsByAddress(walletAddress);
                long amount = this.GetAirdropAmount(assetHoldings, assetValues);
                airdropAmounts.Add(new AirdropAmount(walletAddress, amount));
            }

            return airdropAmounts;
        }

        private long GetAirdropAmount(IEnumerable<AssetHolding> assetHoldings, IDictionary<long, long> assetValues)
        {
            long baseAmount = 0;
            int numberOfAssets = 0;

            foreach (AssetHolding assetHolding in assetHoldings)
            {
                if (assetHolding.AssetId.HasValue &&
                    assetHolding.Amount > 0 &&
                    assetValues.ContainsKey(assetHolding.AssetId.Value))
                {
                    numberOfAssets++;

                    if (assetValues[assetHolding.AssetId.Value] > baseAmount)
                    {
                        baseAmount = assetValues[assetHolding.AssetId.Value];
                    }
                }
            }

            return baseAmount + (numberOfAssets > 0 ? 2 * (numberOfAssets - 1) : 0);
        }

        public override IEnumerable<RetrievedAsset> CheckAssets()
        {
            IEnumerable<AssetHolding> assetHoldings = this._api.GetAssetsByAddress("BNYSETPFTL2657B5RCSW64A3M766GYBVRV5ALOM7F7LIRUZKBEOGF6YSO4");

            List<long> assetIds = assetHoldings.ToList().ConvertAll<long>(ah => ah.AssetId.Value);
            IEnumerable<Asset> assets = this._api.GetAssetById(assetIds);
            List<RetrievedAsset> retrievedAssets = new List<RetrievedAsset>();

            foreach (Asset asset in assets)
            {
                if (asset.Params.UnitName.StartsWith("ALCH"))
                {
                    retrievedAssets.Add(new RetrievedAsset(asset.Params.Name, asset.Params.UnitName, asset.Index.Value));
                }
            }

            return retrievedAssets;
        }
    }

    class AlcheCoinAsset
    {
        public string Name { get; set; }
        public string UnitName { get; set; }
        public long AssetId { get; set; }
        public long Value { get; set; }
    }
}
