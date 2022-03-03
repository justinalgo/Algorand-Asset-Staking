using Algorand.V2.Indexer.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Random
{
    public class CsaRandomFactory : RandomAirdropFactory
    {
        private readonly IIndexerUtils indexerUtils;
        private readonly string[] CreatorAddresses;

        public CsaRandomFactory(IIndexerUtils indexerUtils)
        {
            this.DropAssetId = 226265212;
            this.Decimals = 0;
            this.CreatorAddresses = new string[] { "ACORNOX3WJKR2GX63IJGPC2MV6E7GRNBBEE22BXQ6H7HFMEXLUL3RSEBLY" };
            this.NumberOfWinners = 10;
            this.TotalWinnings = 301886;
            this.indexerUtils = indexerUtils;
        }

        public override async Task<HashSet<ulong>> FetchAssetIds()
        {
            Account account = await this.indexerUtils.GetAccount(this.CreatorAddresses[0]);

            return account.CreatedAssets.Select(ca => ca.Index).ToHashSet();
        }

        public override async Task<IEnumerable<Account>> FetchAccounts()
        {
            IEnumerable<Account> accounts = await this.indexerUtils.GetAccounts(this.DropAssetId);

            accounts = accounts.Where(a => !this.CreatorAddresses.Contains(a.Address));

            return accounts;
        }
    }
}
