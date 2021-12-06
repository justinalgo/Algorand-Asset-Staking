using Algorand.V2.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Util;
using Util.Cosmos;

namespace Airdrop.AirdropFactories.Holdings
{
    public class AlchemonHoldingsFactory : IHoldingsAirdropFactory
    {
        public long AssetId { get; set; }
        public long Decimals { get; set; }
        public string[] CreatorAddresses { get; set; }
        private readonly IAlgoApi api;
        private readonly ICosmos cosmos;
        private readonly long stakeFlagAssetId;

        public AlchemonHoldingsFactory(IAlgoApi api, ICosmos cosmos)
        {
            this.AssetId = 310014962;
            this.Decimals = 0;
            this.CreatorAddresses = new string[] { "OJGTHEJ2O5NXN7FVXDZZEEJTUEQHHCIYIE5MWY6BEFVVLZ2KANJODBOKGA" };
            this.api = api;
            this.cosmos = cosmos;
            this.stakeFlagAssetId = 320570576;
        }

        public async Task<IDictionary<long, long>> FetchAssetValues()
        {
            IEnumerable<AssetValue> values = await cosmos.GetAssetValues("Alchemon");

            Dictionary<long, long> assetValues = values.ToDictionary(av => av.AssetId, av => av.Value);

            return assetValues;
        }

        public IEnumerable<string> FetchWalletAddresses()
        {
            IEnumerable<string> walletAddresses = this.api.GetWalletAddressesWithAsset(this.AssetId, this.stakeFlagAssetId);

            return walletAddresses;
        }

        public async Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts()
        {
            IDictionary<long, long> assetValues = await this.FetchAssetValues();
            ConcurrentBag<AirdropAmount> airdropAmounts = new ConcurrentBag<AirdropAmount>();
            IEnumerable<string> walletAddresses = this.FetchWalletAddresses();

            Parallel.ForEach(walletAddresses, new ParallelOptions { MaxDegreeOfParallelism = 10 }, walletAddress =>
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
                    airdropAmount += (long)(assetValues[miniAssetHolding.AssetId.Value] * Math.Pow(10, this.Decimals) * miniAssetHolding.Amount.Value);
                }
            }

            return airdropAmount;
        }
    }
}
