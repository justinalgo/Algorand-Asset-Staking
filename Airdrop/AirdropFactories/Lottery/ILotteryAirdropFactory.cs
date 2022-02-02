using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Airdrop.AirdropFactories.Lottery
{
    public interface ILotteryAirdropFactory : IAirdropFactory
    {
        public ulong NumberOfWinners { get; set; }
        public ulong TotalWinnings { get; set; }
        public Task<HashSet<ulong>> FetchAssetIds();
        public IEnumerable<(Account, ulong)> GetEligibleWinners(Account account, HashSet<ulong> assetIds);
    }
}
