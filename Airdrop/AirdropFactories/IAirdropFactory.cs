using Algorand.V2.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Airdrop
{
    public interface IAirdropFactory
    {
        public long AssetId { get; set; }
        public long Decimals { get; set; }
        public abstract Task<IDictionary<long, long>> GetAssetValues();
        public abstract IEnumerable<string> FetchWalletAddresses();
        public abstract long GetAssetHoldingsAmount(IEnumerable<AssetHolding> assetHoldings, IDictionary<long, long> assetValues);
        public abstract IEnumerable<AirdropAmount> FetchAirdropAmounts(IDictionary<long, long> assetValues);
    }
}
