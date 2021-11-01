﻿using Algorand.V2.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Util;

namespace Airdrop
{
    public class AlchemonAirdropFactory : IAirdropFactory
    {
        public long AssetId { get; set; }
        public long Decimals { get; set; }
        private readonly IApi api;
        private readonly long stakeFlagAssetId;

        public AlchemonAirdropFactory(IApi api) {
            this.AssetId = 310014962;
            this.Decimals = 0;
            this.api = api;
            this.stakeFlagAssetId = 320570576;
        }

        public IDictionary<long, long> GetAssetValues()
        {
            List<AssetValue> values = JsonSerializer.Deserialize<List<AssetValue>>(File.ReadAllText("C:/Users/ParkG/source/repos/Airdrop/Airdrop/AlchemonValues.json"));

            Dictionary<long, long> assetValues = values.ToDictionary(av => av.AssetId, av => av.Value);

            return assetValues;
        }

        public IEnumerable<string> FetchWalletAddresses()
        {
            IEnumerable<string> walletAddresses = this.api.GetWalletAddressesWithAsset(this.AssetId, this.stakeFlagAssetId);

            return walletAddresses;
        }

        public IEnumerable<AirdropAmount> FetchAirdropAmounts(IDictionary<long, long> assetValues)
        {
            List<AirdropAmount> airdropAmounts = new List<AirdropAmount>();
            IEnumerable<string> walletAddresses = this.FetchWalletAddresses();

            foreach (string walletAddress in walletAddresses)
            {
                IEnumerable<AssetHolding> assetHoldings = this.api.GetAssetsByAddress(walletAddress);
                long amount = this.GetAssetHoldingsAmount(assetHoldings, assetValues);
                if (amount > 0)
                {
                    airdropAmounts.Add(new AirdropAmount(walletAddress, amount));
                }
            }

            return airdropAmounts;
        }

        public long GetAssetHoldingsAmount(IEnumerable<AssetHolding> assetHoldings, IDictionary<long, long> assetValues)
        {
            long baseAmount = 0;
            int numberOfAssets = 0;

            foreach (AssetHolding assetHolding in assetHoldings)
            {
                if (assetHolding.AssetId.HasValue &&
                    assetHolding.Amount.HasValue &&
                    assetHolding.Amount > 0 &&
                    assetValues.ContainsKey(assetHolding.AssetId.Value))
                {
                    numberOfAssets += (int)assetHolding.Amount;

                    if (assetValues[assetHolding.AssetId.Value] > baseAmount)
                    {
                        baseAmount = assetValues[assetHolding.AssetId.Value];
                    }
                }
            }

            return baseAmount + (numberOfAssets > 0 ? 2 * (numberOfAssets - 1) : 0);
        }

        public IEnumerable<RetrievedAsset> CheckAssets()
        {
            IEnumerable<AssetHolding> assetHoldings = this.api.GetAssetsByAddress("BNYSETPFTL2657B5RCSW64A3M766GYBVRV5ALOM7F7LIRUZKBEOGF6YSO4");

            List<long> assetIds = assetHoldings.ToList().ConvertAll<long>(ah => ah.AssetId.Value);
            IEnumerable<Asset> assets = this.api.GetAssetById(assetIds);
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
}
