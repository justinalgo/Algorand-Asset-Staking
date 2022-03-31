using Algorand.V2.Indexer.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public abstract class AcornPartner
    {
        private readonly IIndexerUtils indexerUtils;

        public AcornPartner(IIndexerUtils indexerUtils)
        {
            this.DropAssetId = 226265212;
            this.Decimals = 0;
            this.indexerUtils = indexerUtils;
        }

        public ulong DropAssetId { get; set; }
        public ulong Decimals { get; set; }
        public ulong NumberOfWinners { get; set; }
        public ulong TotalWinnings { get; set; }

        public async Task FetchAirdropUnitCollections(AirdropUnitCollectionManager airdropManager, IEnumerable<Account> accounts)
        {
            HashSet<ulong> assetIds = await this.FetchAssetIds();
            List<(Account, ulong)> eligibleWinners = new List<(Account, ulong)>();

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
        }

        public abstract Task<HashSet<ulong>> FetchAssetIds();

        public async Task<IEnumerable<Account>> FetchAccounts()
        {
            IEnumerable<Account> accounts = await this.indexerUtils.GetAccounts(this.DropAssetId, new ExcludeType[] { ExcludeType.AppsLocalState, ExcludeType.CreatedAssets, ExcludeType.CreatedApps });

            return accounts;
        }

        public IEnumerable<(Account, ulong)> GetEligibleWinners(Account account, HashSet<ulong> assetIds)
        {
            List<(Account, ulong)> eligibleAssets = new List<(Account, ulong)>();

            if (account.Assets != null)
            {
                foreach (AssetHolding assetHolding in account.Assets)
                {
                    if (assetIds.Contains(assetHolding.AssetId) && assetHolding.Amount > 0)
                    {
                        for (ulong i = 0; i < assetHolding.Amount; i++)
                        {
                            eligibleAssets.Add((account, assetHolding.AssetId));
                        }
                    }
                }
            }

            return eligibleAssets;
        }
    }
}
