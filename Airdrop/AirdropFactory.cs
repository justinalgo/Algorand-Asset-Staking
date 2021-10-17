using System;
using System.Collections.Generic;

namespace Airdrop
{
    public abstract class AirdropFactory
    {
        private List<AirdropAmount> AirdropAmounts { get; set; }
        public long AssetId { get; set; }
        public long Decimals { get; set; }

        public AirdropFactory(long assetId, long decimals = 0)
        {
            AirdropAmounts = new List<AirdropAmount>();
            AssetId = assetId;
            Decimals = decimals;
        }

        public abstract IEnumerable<string> FetchWalletAddresses();
        public abstract IEnumerable<AirdropAmount> FetchAirdropAmounts(IDictionary<long, long> assetValues);
    }
}
