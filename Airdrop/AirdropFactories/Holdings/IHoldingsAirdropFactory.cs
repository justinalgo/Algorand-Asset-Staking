using Algorand.V2.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Airdrop.AirdropFactories.Holdings
{
    public interface IHoldingsAirdropFactory : IAirdropFactory
    {
        public Task<IDictionary<long, long>> FetchAssetValues();
        public long GetAssetHoldingsAmount(IEnumerable<AssetHolding> assetHoldings, IDictionary<long, long> assetValues);
    }
}
