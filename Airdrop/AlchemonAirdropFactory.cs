using Algorand.V2.Model;
using Api;
using System.Collections.Generic;

namespace Airdrop
{
    public class AlchemonAirdropFactory : AirdropFactory
    {
        private readonly IApiUtil _apiUtil;
        private readonly long _stakeFlagAssetId;

        public AlchemonAirdropFactory(IApiUtil apiUtil) : base(310014962) {
            this._apiUtil = apiUtil;
            this._stakeFlagAssetId = 320570576;
        }

        public override IEnumerable<string> FetchWalletAddresses()
        {
            IEnumerable<string> walletAddresses = _apiUtil.GetWalletAddressesWithAsset(this.AssetId, this._stakeFlagAssetId);

            return walletAddresses;
        }

        public override IEnumerable<AirdropAmount> FetchAirdropAmounts(IDictionary<long, long> assetValues)
        {
            List<AirdropAmount> airdropAmounts = new List<AirdropAmount>();
            IEnumerable<string> walletAddresses = this.FetchWalletAddresses();

            foreach (string walletAddress in walletAddresses)
            {
                List<AssetHolding> assetHoldings = this._apiUtil.GetAssetsByAddress(walletAddress);
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
    }
}
