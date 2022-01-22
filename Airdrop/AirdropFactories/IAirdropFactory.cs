using System.Collections.Generic;
using System.Threading.Tasks;

namespace Airdrop.AirdropFactories
{
    public interface IAirdropFactory
    {
        public ulong AssetId { get; set; }
        public ulong Decimals { get; set; }
        public Task<IEnumerable<string>> FetchWalletAddresses();
        public Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts();
    }
}
