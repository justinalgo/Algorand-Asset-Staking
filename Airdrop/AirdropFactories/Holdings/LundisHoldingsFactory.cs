using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Holdings
{
    public class LundisHoldingsFactory : HoldingsAirdropFactory
    {
        private readonly IIndexerUtils indexerUtils;

        public LundisHoldingsFactory(IIndexerUtils indexerUtils)
        {
            this.DropAssetId = 658399558;
            this.Decimals = 0;
            this.CreatorAddresses = new string[] {
                "PFDZQWMRT2KTJTB3VUGDOILNCNSB63ILZL4XMBHYECBOV24LGTID4YRFPM",
                "7PVEEP2CS77VJEYZGW2IIGZ63P5CO557XRNKBRPTIREKLET7A4G62W4CQA",
                "TXZ3AKZLIKNNT3OBQOMTSYWC7AK7CIVSZZIDCSTONXNTX44LQIRU6ELFDA",
            };
            this.indexerUtils = indexerUtils;
        }

        public override async Task<IEnumerable<Account>> FetchAccounts()
        {
            IEnumerable<Account> accounts = await this.indexerUtils.GetAccounts(this.DropAssetId, new ExcludeType[] { ExcludeType.CreatedAssets, ExcludeType.CreatedApps, ExcludeType.AppsLocalState });

            return accounts;
        }

        public override async Task<IDictionary<ulong, ulong>> FetchAssetValues()
        {
            Dictionary<ulong, ulong> assetValues = new Dictionary<ulong, ulong>();

            foreach (string creatorAddress in this.CreatorAddresses)
            {
                IEnumerable<Asset> assets = await this.indexerUtils.GetCreatedAssets(creatorAddress);

                foreach (Asset asset in assets)
                {
                    assetValues.Add(asset.Index, 50);
                }
            }

            return assetValues;
        }
    }
}
