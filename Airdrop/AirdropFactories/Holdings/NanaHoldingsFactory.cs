using Algorand.V2.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util;
using Util.Cosmos;

namespace Airdrop.AirdropFactories.Holdings
{
    public class NanaHoldingsFactory : IHoldingsAirdropFactory
    {
        public long AssetId { get; set; }
        public long Decimals { get; set; }
        private readonly IAlgoApi api;
        private readonly ICosmos cosmos;

        public NanaHoldingsFactory(IAlgoApi api, ICosmos cosmos)
        {
            this.AssetId = 418706707;
            this.Decimals = 0;
            this.api = api;
            this.cosmos = cosmos;
        }

        public async Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts()
        {
            IDictionary<long, long> assetValues = await this.FetchAssetValues();
            ConcurrentBag<AirdropAmount> airdropAmounts = new ConcurrentBag<AirdropAmount>();
            IEnumerable<string> walletAddresses = this.FetchWalletAddresses();

            Parallel.ForEach(walletAddresses, walletAddress =>
            {
                IEnumerable<AssetHolding> assetHoldings = this.api.GetAssetsByAddress(walletAddress);
                long amount = this.GetAssetHoldingsAmount(assetHoldings, assetValues);
                if (amount > 0)
                {
                    airdropAmounts.Add(new AirdropAmount(walletAddress, this.AssetId, amount));
                }
            });

            return airdropAmounts;
        }

        public async Task<IDictionary<long, long>> FetchAssetValues()
        {
            IEnumerable<AssetValue> values = await cosmos.GetAssetValues("NanaCoin");

            Dictionary<long, long> assetValues = values.ToDictionary(av => av.AssetId, av => av.Value);

            return assetValues;
        }

        public IEnumerable<string> FetchWalletAddresses()
        {
            IEnumerable<string> walletAddresses = this.api.GetWalletAddressesWithAsset(this.AssetId);

            return walletAddresses;
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
    }
}
