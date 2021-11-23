using System.Collections.Generic;
using System.Threading.Tasks;

namespace Airdrop.AirdropFactories
{
    public interface IAirdropFactory
    {
        public long AssetId { get; set; }
        public long Decimals { get; set; }
        public IEnumerable<string> FetchWalletAddresses();
        public Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts();
    }
}
