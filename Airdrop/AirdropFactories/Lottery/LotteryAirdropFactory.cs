using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Airdrop.AirdropFactories.Lottery
{
    public abstract class LotteryAirdropFactory : ILotteryAirdropFactory
    {
        public ulong NumberOfWinners { get; set; }
        public ulong AssetId { get; set; }
        public ulong Decimals { get; set; }
        public ulong TotalWinnings { get; set; }
        public abstract Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts();

        public abstract Task<HashSet<ulong>> FetchAssetIds();

        public abstract Task<IEnumerable<Account>> FetchAccounts();

        public IEnumerable<(Account, ulong)> GetEligibleWinners(Account account, HashSet<ulong> assetIds)
        {
            List<(Account, ulong)> eligibleAssets = new List<(Account, ulong)>();

            if (account.Assets != null)
            {
                foreach (AssetHolding assetHolding in account.Assets)
                {
                    if (assetIds.Contains(assetHolding.AssetId))
                    {
                        eligibleAssets.Add((account, assetHolding.AssetId));
                    }
                }
            }

            return eligibleAssets;
        }
    }
}
