using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class TinyWhalesPartnerFactory : AcornPartner
    {
        private readonly IIndexerUtils indexerUtils;
        private readonly string[] CreatorAddresses;
        public ulong[] RevokedAssets { get; }

        public TinyWhalesPartnerFactory(IIndexerUtils indexerUtils, ulong totalWinnings) : base(indexerUtils)
        {
            this.CreatorAddresses = new string[] {
                "WALEJYNKT5LDSHBR43Y2PCDYCIUA4LQ4ZCMJ2MAYODLJG5YUVAAP4YX7UQ",
                "WALEHCLT5GH4PXZAJHFAJIKTBOVOJVQCXXNIZAQICD27MGNWPDMV2TK3CI",
                "WALEHTQCNBJ7JTIG5JREIIC3HMZVDYKMM5L6DHHMVKU4DOSLRQOBHO45PQ" };
            this.RevokedAssets = new ulong[]
            {
                513799026, 516993196, 516995986, 517005315, 517007356, 517011087, 517016841, 517018556, 517034102, 517035626
            };
            this.NumberOfWinners = 10;
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
