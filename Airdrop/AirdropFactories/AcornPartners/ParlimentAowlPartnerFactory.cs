using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class ParlimentAowlPartnerFactory : AcornPartner
    {
        private readonly IIndexerUtils indexerUtils;
        private readonly string[] CreatorAddresses;
        public ulong[] RevokedAssets { get; }

        public ParlimentAowlPartnerFactory(IIndexerUtils indexerUtils, ulong totalWinnings) : base(indexerUtils)
        {
            this.CreatorAddresses = new string[] {
                "AOWLLUX3BBLDV6KUZYQ7FBZTIWGWRRJO6B5XL2DFQ6WLITHUK26OO7IGMI",
                "AOWL3ZOC55JJDB5LU5CCIVK3WITVXMVZU6PV2CQQOLXUNCTOJQ6VJCPYAU",
                "AOWLDAJSAIHNR4J2TKB4U32Z73O7W52JGJUGUR5RW6ZH46G3ET4IFNNGQ4"};
            this.RevokedAssets = new ulong[0];
            this.NumberOfWinners = 8;
            this.TotalWinnings = totalWinnings;
            this.indexerUtils = indexerUtils;
        }

        public override async Task<HashSet<ulong>> FetchAssetIds()
        {
            HashSet<ulong> assetIds = new HashSet<ulong>();

            foreach (string creatorAddress in this.CreatorAddresses)
            {
                Account account = await this.indexerUtils.GetAccount(creatorAddress);

                foreach (Asset asset in account.CreatedAssets)
                {
                    if (!this.RevokedAssets.Contains(asset.Index))
                    {
                        assetIds.Add(asset.Index);
                    }
                }
            }

            return assetIds;
        }
    }
}
