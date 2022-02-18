using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class SwappyPartnerFactory : AcornPartner
    {
        private readonly IIndexerUtils indexerUtils;
        private readonly string[] CreatorAddresses;
        public ulong[] RevokedAssets { get; }

        public SwappyPartnerFactory(IIndexerUtils indexerUtils, ulong totalWinnings) : base(indexerUtils)
        {
            this.CreatorAddresses = new string[] {
                "GANGDMU6TWCMQPBLXBCW6ZEBZJ35VK2665CASIGV2ARSVZB7VIOKWAA5UU",};
            this.RevokedAssets = new ulong[0];
            this.NumberOfWinners = 50;
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
