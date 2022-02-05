using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airdrop.AirdropFactories.Random
{
    public abstract class RandomAirdropFactory : IRandomAirdropFactory
    {
        public ulong NumberOfWinners { get; set; }
        public ulong DropAssetId { get; set; }
        public ulong Decimals { get; set; }
        public ulong TotalWinnings { get; set; }

        public async Task<IEnumerable<AirdropUnitCollection>> FetchAirdropUnitCollections()
        {
            HashSet<ulong> assetIds = await this.FetchAssetIds();
            IEnumerable<Account> accounts = await this.FetchAccounts();
            List<(Account, ulong)> eligibleWinners = new List<(Account, ulong)>();
            AirdropUnitCollectionManager airdropManager = new AirdropUnitCollectionManager();

            foreach (Account account in accounts)
            {
                eligibleWinners.AddRange(this.GetEligibleWinners(account, assetIds));
            }

            System.Random random = new System.Random();
            ulong i = 0;

            while (i < this.NumberOfWinners && eligibleWinners.Count > 0)
            {
                int winnerIndex = random.Next(eligibleWinners.Count);

                (Account, ulong) winnerInfo = eligibleWinners.ElementAt(winnerIndex);

                Account winner = winnerInfo.Item1;
                ulong winningAssetId = winnerInfo.Item2;

                eligibleWinners.RemoveAt(winnerIndex);

                airdropManager.AddAirdropUnit(new AirdropUnit(
                    winner.Address,
                    this.DropAssetId,
                    winningAssetId,
                    this.TotalWinnings / this.NumberOfWinners,
                    numberOfSourceAsset: 1));

                i++;
            }

            return airdropManager.GetAirdropUnitCollections();
        }

        public abstract Task<HashSet<ulong>> FetchAssetIds();

        public abstract Task<IEnumerable<Account>> FetchAccounts();

        public IEnumerable<(Account, ulong)> GetEligibleWinners(Account account, HashSet<ulong> assetIds)
        {
            List<(Account, ulong)> eligibleAssets = new List<(Account, ulong)>();

            if (account.Assets != null)
            {
                foreach (AssetHolding assetHolding in account.Assets)
                {
                    if (assetIds.Contains(assetHolding.AssetId) && assetHolding.Amount > 0)
                    {
                        eligibleAssets.Add((account, assetHolding.AssetId));
                    }
                }
            }

            return eligibleAssets;
        }
    }
}
