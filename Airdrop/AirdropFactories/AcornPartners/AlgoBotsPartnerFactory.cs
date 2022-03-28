using Algorand.V2.Indexer.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class AlgoBotsPartnerFactory : AcornPartner
    {
        private readonly IIndexerUtils indexerUtils;
        private readonly string[] CreatorAddresses;
        public string AssetPrefix { get; }

        public AlgoBotsPartnerFactory(IIndexerUtils indexerUtils, ulong totalWinnings) : base(indexerUtils)
        {
            this.CreatorAddresses = new string[] {
                "NZTRBMNPONCBXZX3MIQUKCUSN4ONAGKLQN445XC7NMQ2WR7HLVWYRTVRRY"};
            this.AssetPrefix = "Algobot";
            this.NumberOfWinners = 10;
            this.TotalWinnings = totalWinnings;
            this.indexerUtils = indexerUtils;
        }

        public override async Task<HashSet<ulong>> FetchAssetIds()
        {
            HashSet<ulong> assetIds = new HashSet<ulong>();

            foreach (string creatorAddress in this.CreatorAddresses)
            {
                Account account = await this.indexerUtils.GetAccount(creatorAddress, new ExcludeType[] { ExcludeType.AppsLocalState, ExcludeType.Assets, ExcludeType.CreatedApps });

                foreach (Asset asset in account.CreatedAssets)
                {
                    if ((asset.Params.UnitName?.Contains(this.AssetPrefix)).GetValueOrDefault())
                    {
                        assetIds.Add(asset.Index);
                    }
                }
            }

            return assetIds;
        }
    }
}
