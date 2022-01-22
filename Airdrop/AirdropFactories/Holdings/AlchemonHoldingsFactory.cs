using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Util.Cosmos;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Holdings
{
    public class AlchemonHoldingsFactory : IHoldingsAirdropFactory
    {
        public ulong AssetId { get; set; }
        public ulong Decimals { get; set; }
        public string[] CreatorAddresses { get; set; }
        private readonly IIndexerUtils indexerUtils;
        private readonly ICosmos cosmos;
        private readonly ulong stakeFlagAssetId;

        public AlchemonHoldingsFactory(IIndexerUtils indexerUtils, ICosmos cosmos)
        {
            this.AssetId = 310014962;
            this.Decimals = 0;
            this.CreatorAddresses = new string[] { "OJGTHEJ2O5NXN7FVXDZZEEJTUEQHHCIYIE5MWY6BEFVVLZ2KANJODBOKGA" };
            this.indexerUtils = indexerUtils;
            this.cosmos = cosmos;
            this.stakeFlagAssetId = 320570576;
        }

        public async Task<IDictionary<ulong, ulong>> FetchAssetValues()
        {
            IEnumerable<AssetValue> values = await cosmos.GetAssetValues("Alchemon");

            Dictionary<ulong, ulong> assetValues = values.ToDictionary(av => av.AssetId, av => av.Value);

            return assetValues;
        }

        public async Task<IEnumerable<string>> FetchWalletAddresses()
        {
            IEnumerable<string> walletAddresses = await this.indexerUtils.GetWalletAddresses(this.AssetId, this.stakeFlagAssetId);

            return walletAddresses;
        }

        public async Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts()
        {
            IDictionary<ulong, ulong> assetValues = await this.FetchAssetValues();
            ConcurrentBag<AirdropAmount> airdropAmounts = new ConcurrentBag<AirdropAmount>();
            IEnumerable<string> walletAddresses = await this.FetchWalletAddresses();

            Parallel.ForEach(walletAddresses, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async walletAddress =>
            {
                Account account = await this.indexerUtils.GetAccount(walletAddress);
                IEnumerable<AssetHolding> assetHoldings = account.Assets;

                if (assetHoldings != null)
                {
                    ulong amount = this.GetAssetHoldingsAmount(assetHoldings, assetValues);

                    if (amount > 0)
                    {
                        airdropAmounts.Add(new AirdropAmount(walletAddress, this.AssetId, amount));
                    }
                }
            });

            return airdropAmounts;
        }

        public ulong GetAssetHoldingsAmount(IEnumerable<AssetHolding> assetHoldings, IDictionary<ulong, ulong> assetValues)
        {
            ulong airdropAmount = 0;

            foreach (AssetHolding miniAssetHolding in assetHoldings)
            {
                if (miniAssetHolding.Amount > 0 &&
                    assetValues.ContainsKey(miniAssetHolding.AssetId))
                {
                    airdropAmount += (ulong)(assetValues[miniAssetHolding.AssetId] * Math.Pow(10, this.Decimals) * miniAssetHolding.Amount);
                }
            }

            return airdropAmount;
        }
    }
}
