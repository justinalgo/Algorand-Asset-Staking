using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Lottery
{
    public class CsaLotteryFactory : LotteryAirdropFactory
    {
        private readonly IIndexerUtils indexerUtils;
        private readonly string[] CreatorAddresses;

        public CsaLotteryFactory(IIndexerUtils indexerUtils)
        {
            this.AssetId = 226265212;
            this.Decimals = 0;
            this.CreatorAddresses = new string[] { "ACORNOX3WJKR2GX63IJGPC2MV6E7GRNBBEE22BXQ6H7HFMEXLUL3RSEBLY" };
            this.NumberOfWinners = 10;
            this.TotalWinnings = 221312;
            this.indexerUtils = indexerUtils;
        }

        public override async Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts()
        {
            HashSet<ulong> assetIds = await this.FetchAssetIds();
            IEnumerable<Account> accounts = await this.FetchAccounts();
            List<(Account, ulong)> eligibleWinners = new List<(Account, ulong)>();
            List<AirdropAmount> winners = new List<AirdropAmount>();

            foreach (Account account in accounts) {
                eligibleWinners.AddRange(this.GetEligibleWinners(account, assetIds));
            }

            Random random = new Random();
            ulong i = 0;

            while (i < this.NumberOfWinners && eligibleWinners.Count > 0)
            {
                int winnerIndex = random.Next(eligibleWinners.Count);

                (Account, ulong) winner = eligibleWinners.ElementAt(winnerIndex);
                eligibleWinners.RemoveAt(winnerIndex);

                winners.Add(new AirdropAmount(winner.Item1.Address, this.AssetId, this.TotalWinnings / this.NumberOfWinners));

                i++;
            }

            return winners;
        }

        public override async Task<HashSet<ulong>> FetchAssetIds()
        {
            Account account = await this.indexerUtils.GetAccount(this.CreatorAddresses[0]);

            return account.CreatedAssets.Select(ca => ca.Index).ToHashSet();
        }

        public override async Task<IEnumerable<Account>> FetchAccounts()
        {
            IEnumerable<Account> accounts = await this.indexerUtils.GetAccounts(this.AssetId);

            return accounts;
        }
    }
}
