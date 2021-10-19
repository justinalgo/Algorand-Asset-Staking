using Algorand.V2.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Util;

namespace Airdrop
{
    public class CryptoBunnyAirdropFactory : AirdropFactory
    {
        private readonly IApi _api;
        public CryptoBunnyAirdropFactory(IApi api) : base(329532956)
        {
            this._api = api;
        }
        public override IEnumerable<RetrievedAsset> CheckAssets()
        {
            IEnumerable<AssetHolding> assetHoldings = this._api.GetAssetsByAddress("BNYSETPFTL2657B5RCSW64A3M766GYBVRV5ALOM7F7LIRUZKBEOGF6YSO4");

            List<long> assetIds = assetHoldings.ToList().ConvertAll<long>(ah => ah.AssetId.Value);
            IEnumerable<Asset> assets = this._api.GetAssetById(assetIds);
            List<RetrievedAsset> retrievedAssets = new List<RetrievedAsset>();

            foreach (Asset asset in assets)
            {
                if (asset.Params.UnitName.StartsWith("BNYL") || asset.Params.UnitName.StartsWith("BNYO"))
                {
                    retrievedAssets.Add(new RetrievedAsset(asset.Params.Name, asset.Params.UnitName, asset.Index.Value));
                }
            }

            return retrievedAssets;
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

        public override IEnumerable<string> FetchWalletAddresses()
        {
            IEnumerable<string> walletAddresses = this._api.GetWalletAddressesWithAsset(this.AssetId);

            return walletAddresses;
        }

        public override IDictionary<long, long> GetAssetValues()
        {
            List<CryptoBunnyAsset> cryptoBunnyAssets = JsonConvert.DeserializeObject<List<CryptoBunnyAsset>>(File.ReadAllText("C:/Users/ParkG/source/repos/Airdrop/Airdrop/CryptoBunnyValues.json"));

            Dictionary<long, long> assetValues = cryptoBunnyAssets.ToDictionary(av => av.AssetId, av => av.Value);

            return assetValues;
        }

        private long GetAirdropAmount(IEnumerable<AssetHolding> assetHoldings, IDictionary<long, long> assetValues)
        {
            long airdropAmount = 0;

            foreach (AssetHolding miniAssetHolding in assetHoldings)
            {
                if (miniAssetHolding.AssetId.HasValue &&
                    miniAssetHolding.Amount.HasValue &&
                    miniAssetHolding.Amount > 0 &&
                    assetValues.ContainsKey(miniAssetHolding.AssetId.Value))
                {
                    airdropAmount += (long)miniAssetHolding.Amount.Value;
                }
            }

            return airdropAmount;
        }
    }

    class CryptoBunnyAsset
    {
        public string Name { get; set; }
        public string UnitName { get; set; }
        public long AssetId { get; set; }
        public long Value { get; set; }
    }
}
