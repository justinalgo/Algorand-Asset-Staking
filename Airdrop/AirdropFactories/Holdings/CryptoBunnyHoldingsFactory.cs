using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils;
using Utils.Cosmos;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Holdings
{
    public class CryptoBunnyHoldingsFactory : HoldingsAirdropFactory
    {
        private readonly IIndexerUtils indexerUtils;
        private readonly ICosmos cosmos;

        public CryptoBunnyHoldingsFactory(IIndexerUtils indexerUtils, ICosmos cosmos)
        {
            this.DropAssetId = 329532956;
            this.Decimals = 0;
            this.CreatorAddresses = new string[] { "BNYSETPFTL2657B5RCSW64A3M766GYBVRV5ALOM7F7LIRUZKBEOGF6YSO4" };
            this.indexerUtils = indexerUtils;
            this.cosmos = cosmos;
        }

        public override async Task<IEnumerable<Account>> FetchAccounts()
        {
            IEnumerable<Account> accounts = await this.indexerUtils.GetAccounts(this.DropAssetId);

            return accounts;
        }

        public override async Task<IDictionary<ulong, ulong>> FetchAssetValues()
        {
            IEnumerable<AssetValue> assets = await cosmos.GetAssetValues("CryptoBunny");

            Dictionary<ulong, ulong> assetValues = assets.ToDictionary(av => av.AssetId, av => av.Value);

            return assetValues;
        }
    }
}
