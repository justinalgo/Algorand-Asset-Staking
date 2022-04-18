using Algorand.V2.Algod.Model;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Random
{
    public class CsaRandomFactory : RandomAirdropFactory
    {
        private readonly IIndexerUtils indexerUtils;
        private readonly IAlgodUtils algodUtils;
        private readonly string[] CreatorAddresses;

        public CsaRandomFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils)
        {
            this.DropAssetId = 226265212;
            this.Decimals = 0;
            this.CreatorAddresses = new string[] { "ACORNOX3WJKR2GX63IJGPC2MV6E7GRNBBEE22BXQ6H7HFMEXLUL3RSEBLY" };
            this.NumberOfWinners = 10;
            this.TotalWinnings = 200000;
            this.indexerUtils = indexerUtils;
            this.algodUtils = algodUtils;
        }

        public override async Task<HashSet<ulong>> FetchAssetIds()
        {
            var account = await this.algodUtils.GetAccount(this.CreatorAddresses[0]);

            return account.CreatedAssets.Select(ca => ca.Index).ToHashSet();
        }

        public override async Task<IEnumerable<Account>> FetchAccounts()
        {
            IEnumerable<string> addresses = await this.indexerUtils.GetWalletAddresses(this.DropAssetId);

            ConcurrentBag<Account> accounts = new ConcurrentBag<Account>();

            Parallel.ForEach<string>(addresses, new ParallelOptions { MaxDegreeOfParallelism = 10 }, address =>
            {
                var account = this.algodUtils.GetAccount(address).Result;
                accounts.Add(account);
            });

            return accounts.Where(a => !this.CreatorAddresses.Contains(a.Address));
        }
    }
}
