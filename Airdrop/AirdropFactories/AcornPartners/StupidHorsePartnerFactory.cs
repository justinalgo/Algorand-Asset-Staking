using Algorand.V2.Indexer.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class StupidHorsePartnerFactory : AcornPartner
    {
        private readonly IIndexerUtils indexerUtils;
        private readonly string[] CreatorAddresses;
        public string AssetPrefix { get; }

        public StupidHorsePartnerFactory(IIndexerUtils indexerUtils, ulong totalWinnings) : base(indexerUtils)
        {
            this.CreatorAddresses = new string[] {
                "GLOW7AKCAZXWQRPI6Q7OCVAO75H45AIYMTDEH3VNPETKYFXMNHAMQOVMS4",
                "STPD5WZ7DMF2RBBGROROWS6U2HNKC4SOHZXTFDTRIWHTXQ46TA7HU3A2SI"};
            this.AssetPrefix = "HORSE";
            this.NumberOfWinners = 2;
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
                    if (asset.Params.UnitName.StartsWith(this.AssetPrefix))
                    {
                        assetIds.Add(asset.Index);
                    }
                }
            }

            return assetIds;
        }
    }
}
