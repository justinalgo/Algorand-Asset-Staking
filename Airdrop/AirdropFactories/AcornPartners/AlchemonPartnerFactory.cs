using Algorand.V2.Indexer.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Cosmos;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class AlchemonPartnerFactory
    {
        public ulong TotalWinnings { get; set; }

        private readonly IIndexerUtils indexerUtils;
        private readonly ICosmos cosmos;
        private readonly string[] CreatorAddresses;
        public ulong[] RevokedAssets { get; }
        public ulong DropAssetId { get; set; }
        public ulong Decimals { get; set; }
        public AlchemonPartnerFactory(IIndexerUtils indexerUtils, ICosmos cosmos, ulong totalWinnings)
        {
            this.DropAssetId = 226265212;
            this.Decimals = 0;
            this.CreatorAddresses = new string[] {
                "OJGTHEJ2O5NXN7FVXDZZEEJTUEQHHCIYIE5MWY6BEFVVLZ2KANJODBOKGA"};
            this.TotalWinnings = totalWinnings;
            this.indexerUtils = indexerUtils;
            this.cosmos = cosmos;
        }

        public async Task FetchAirdropUnitCollections(AirdropUnitCollectionManager airdropManager, IEnumerable<Account> accounts)
        {
            System.Random random = new System.Random();
            IEnumerable<AssetValue> assetValues = await this.cosmos.GetAssetValues("Alchemon");
            ulong assetId = assetValues.ElementAt(random.Next(assetValues.Count())).AssetId;

            List<Account> newAccounts = new List<Account>();
            ulong numCards = 0;

            foreach (Account account in accounts)
            {
                AssetHolding asset = account.Assets?.FirstOrDefault(a => a.AssetId == assetId);

                if (asset != null && asset.Amount > 0)
                {
                    numCards += asset.Amount;
                    newAccounts.Add(account);
                }
            }

            double val = this.TotalWinnings / (double)numCards;

            foreach (Account account in newAccounts)
            {
                AssetHolding asset = account.Assets?.FirstOrDefault(a => a.AssetId == assetId);

                airdropManager.AddAirdropUnit(new AirdropUnit(
                    account.Address,
                    this.DropAssetId,
                    assetId,
                    val,
                    asset.Amount,
                    true));
            }
        }
    }
}
