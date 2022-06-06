using Algorand.V2.Algod.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Cosmos;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Holdings
{
    public class AlchemonHoldingsFactory : HoldingsAirdropFactory
    {
        private readonly ICosmos cosmos;
        private readonly ulong stakeFlagAssetId;

        public AlchemonHoldingsFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ICosmos cosmos) : base(indexerUtils, algodUtils)
        {
            this.DropAssetId = 310014962;
            this.Decimals = 0;
            this.CreatorAddresses = new string[] { "OJGTHEJ2O5NXN7FVXDZZEEJTUEQHHCIYIE5MWY6BEFVVLZ2KANJODBOKGA" };
            this.RevokedAddresses = new string[] {
                "LEMO5ZPXGACO25UN4GFHWFRHP2MZNJTJL7OV3HYJU7KZF2TL7CHWZIYEWU",
                "5W3QB7A7BFX2MD7XRMD3FLYEBS4AMOVFHRL5QAOP4QQC227L2GVIJIWNNM",
                "C5UMHCZBPPVFUSPHCZT6YYFXB6IXTF7Z57JQ7SHUGIC5PBU7BSLNPKSSSY",
                "OJGTHEJ2O5NXN7FVXDZZEEJTUEQHHCIYIE5MWY6BEFVVLZ2KANJODBOKGA",
            };
            this.cosmos = cosmos;
            this.stakeFlagAssetId = 320570576;
        }

        public override async Task<IDictionary<ulong, ulong>> FetchAssetValues()
        {
            IEnumerable<AssetValue> values = await cosmos.GetAssetValues("Alchemon");

            Dictionary<ulong, ulong> assetValues = values.ToDictionary(av => av.AssetId, av => av.Value);

            return assetValues;
        }

        public override async Task<IEnumerable<Account>> FetchAccounts()
        {
            IEnumerable<string> addresses = await this.IndexerUtils.GetWalletAddressesIntersect(new ulong[] { this.DropAssetId, this.stakeFlagAssetId });

            ConcurrentBag<Account> accounts = new ConcurrentBag<Account>();

            Parallel.ForEach<string>(addresses, new ParallelOptions { MaxDegreeOfParallelism = 10 }, address =>
            {
                var account = this.AlgodUtils.GetAccount(address).Result;
                accounts.Add(account);
            });

            if (this.RevokedAddresses != null)
            {
                return accounts.Where(a => !this.RevokedAddresses.Contains(a.Address));
            }

            return accounts;
        }
    }
}
