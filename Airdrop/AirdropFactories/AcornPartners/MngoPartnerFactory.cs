using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class MngoPartnerFactory : AcornPartner
    {
        private readonly IIndexerUtils indexerUtils;
        private readonly string[] CreatorAddresses;
        public ulong[] RevokedAssets { get; }

        public MngoPartnerFactory(IIndexerUtils indexerUtils, ulong totalWinnings) : base(indexerUtils)
        {
            this.CreatorAddresses = new string[] {
                "MNGOLDXO723TDRM6527G7OZ2N7JLNGCIH6U2R4MOCPPLONE3ZATOBN7OQM", 
                "MNGORTG4A3SLQXVRICQXOSGQ7CPXUPMHZT3FJZBIZHRYAQCYMEW6VORBIA", 
                "MNGOZ3JAS3C4QTGDQ5NVABUEZIIF4GAZY52L3EZE7BQIBFTZCNLQPXHRHE", 
                "MNGO4JTLBN64PJLWTQZYHDMF2UBHGJGW5L7TXDVTJV7JGVD5AE4Y3HTEZM"};
            this.RevokedAssets = new ulong[0];
            this.NumberOfWinners = 150;
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
