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
    public class AlchemonHoldingsFactory : HoldingsAirdropFactory
    {
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

        public override async Task<IDictionary<ulong, ulong>> FetchAssetValues()
        {
            IEnumerable<AssetValue> values = await cosmos.GetAssetValues("Alchemon");

            Dictionary<ulong, ulong> assetValues = values.ToDictionary(av => av.AssetId, av => av.Value);

            return assetValues;
        }

        public override async Task<IEnumerable<string>> FetchWalletAddresses()
        {
            IEnumerable<string> walletAddresses = await this.indexerUtils.GetWalletAddresses(this.AssetId, this.stakeFlagAssetId);

            return walletAddresses;
        }

        public override async Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts()
        {
            IDictionary<ulong, ulong> assetValues = await this.FetchAssetValues();
            ConcurrentBag<AirdropAmount> airdropAmounts = new ConcurrentBag<AirdropAmount>();
            IEnumerable<string> walletAddresses = await this.FetchWalletAddresses();

            Parallel.ForEach(walletAddresses, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async walletAddress =>
            {
                Account account = await this.indexerUtils.GetAccount(walletAddress);
                IEnumerable<AssetHolding> assetHoldings = account.Assets;
                
                ulong amount = this.GetAssetHoldingsAmount(assetHoldings, assetValues);

                if (amount > 0)
                {
                    airdropAmounts.Add(new AirdropAmount(walletAddress, this.AssetId, amount));
                }
            });

            return airdropAmounts;
        }
    }
}
