using Algorand.V2.Algod.Model;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Random
{
    public abstract class RandomAirdropFactory
    {
        public IIndexerUtils IndexerUtils { get; }
        public IAlgodUtils AlgodUtils { get; }
        public string[] CreatorAddresses { get; set; }
        public ulong DropAssetId { get; set; }
        public ulong Decimals { get; set; }
        public ulong NumberOfWinners { get; set; }
        public ulong TotalWinnings { get; set; }
        public RandomAirdropFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils)
        {
            this.IndexerUtils = indexerUtils;
            this.AlgodUtils = algodUtils;
        }

        public async Task<IEnumerable<AirdropUnitCollection>> FetchAirdropUnitCollections()
        {
            HashSet<ulong> assetIds = await FetchAssetIds();
            IEnumerable<Account> accounts = await FetchAccounts();
            List<(Account, ulong)> eligibleWinners = new List<(Account, ulong)>();
            AirdropUnitCollectionManager airdropManager = new AirdropUnitCollectionManager();

            foreach (Account account in accounts)
            {
                eligibleWinners.AddRange(GetEligibleWinners(account, assetIds));
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


        public async Task<HashSet<ulong>> FetchAssetIds()
        {
            var account = await AlgodUtils.GetAccount(this.CreatorAddresses[0]);

            return account.CreatedAssets.Select(ca => ca.Index).ToHashSet();
        }

        public async Task<IEnumerable<Account>> FetchAccounts()
        {
            IEnumerable<string> addresses = await IndexerUtils.GetWalletAddresses(this.DropAssetId);

            ConcurrentBag<Account> accounts = new ConcurrentBag<Account>();

            Parallel.ForEach<string>(addresses, new ParallelOptions { MaxDegreeOfParallelism = 10 }, address =>
            {
                var account = AlgodUtils.GetAccount(address).Result;
                accounts.Add(account);
            });

            return accounts.Where(a => !this.CreatorAddresses.Contains(a.Address));
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
