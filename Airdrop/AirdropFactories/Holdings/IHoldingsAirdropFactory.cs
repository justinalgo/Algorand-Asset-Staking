using Algorand.V2.Indexer.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Airdrop.AirdropFactories.Holdings
{
    public interface IHoldingsAirdropFactory : IAirdropFactory
    {
        public string[] CreatorAddresses { get; set; }
        public Task<IDictionary<ulong, ulong>> FetchAssetValues();
        public ulong GetAssetHoldingsAmount(IEnumerable<AssetHolding> assetHoldings, IDictionary<ulong, ulong> assetValues);
    }
}
