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
        private readonly string[] revokedAddresses;

        public AlchemonHoldingsFactory(IIndexerUtils indexerUtils, ICosmos cosmos)
        {
            this.AssetId = 310014962;
            this.Decimals = 0;
            this.CreatorAddresses = new string[] { "OJGTHEJ2O5NXN7FVXDZZEEJTUEQHHCIYIE5MWY6BEFVVLZ2KANJODBOKGA" };
            this.revokedAddresses = new string[] {
                "LEMO5ZPXGACO25UN4GFHWFRHP2MZNJTJL7OV3HYJU7KZF2TL7CHWZIYEWU",
                "5W3QB7A7BFX2MD7XRMD3FLYEBS4AMOVFHRL5QAOP4QQC227L2GVIJIWNNM",
                "C5UMHCZBPPVFUSPHCZT6YYFXB6IXTF7Z57JQ7SHUGIC5PBU7BSLNPKSSSY",
            };
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

            walletAddresses.Except(this.revokedAddresses);

            return walletAddresses;
        }

        public override async Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts()
        {
            IDictionary<ulong, ulong> assetValues = await this.FetchAssetValues();
            ConcurrentBag<AirdropAmount> airdropAmounts = new ConcurrentBag<AirdropAmount>();
            IEnumerable<string> walletAddresses = await this.FetchWalletAddresses();

            Parallel.ForEach(walletAddresses, new ParallelOptions { MaxDegreeOfParallelism = 10 }, walletAddress =>
            {
                Account account = this.indexerUtils.GetAccount(walletAddress).Result;
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
