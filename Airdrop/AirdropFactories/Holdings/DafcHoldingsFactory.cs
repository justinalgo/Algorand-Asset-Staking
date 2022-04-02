using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Holdings
{
    public class DafcHoldingsFactory : RandHoldingsAirdropFactory
    {
        private readonly IIndexerUtils indexerUtils;

        public DafcHoldingsFactory(IIndexerUtils indexerUtils, IHttpClientFactory httpClientFactory) : base(httpClientFactory.CreateClient())
        {
            this.DropAssetId = 624128801;
            this.Decimals = 0;
            this.CreatorAddresses = new string[] { "DAFCFSH5IFPO4DWDUHHZFV6UESAJB4IJRZ3R4LXATMDHQ2FFIDEGXBBPHA" };
            this.indexerUtils = indexerUtils;
        }

        public override async Task<IEnumerable<Account>> FetchAccounts()
        {
            IEnumerable<Account> accounts = await this.indexerUtils.GetAccounts(this.DropAssetId, new ExcludeType[] { ExcludeType.CreatedAssets, ExcludeType.CreatedApps, ExcludeType.AppsLocalState });

            return accounts;
        }

        public override async Task<IDictionary<ulong, ulong>> FetchAssetValues()
        {
            Dictionary<ulong, ulong> assetIds = new Dictionary<ulong, ulong>();

            foreach (string creatorAddress in this.CreatorAddresses)
            {
                Account account = await this.indexerUtils.GetAccount(creatorAddress);

                foreach (Asset asset in account.CreatedAssets)
                {
                    assetIds.Add(asset.Index, 1);
                }
            }

            return assetIds;
        }
    }
}
