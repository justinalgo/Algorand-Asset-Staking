using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Airdrop.AirdropFactories.Holdings
{
    public abstract class HoldingsAirdropFactory : IHoldingsAirdropFactory
    {
        public string[] CreatorAddresses { get; set; }
        public ulong DropAssetId { get; set; }
        public ulong Decimals { get; set; }
        public abstract Task<IDictionary<ulong, ulong>> FetchAssetValues();
        public abstract Task<IEnumerable<Account>> FetchAccounts();

        public async Task<IEnumerable<AirdropUnitCollection>> FetchAirdropUnitCollections()
        {
            IDictionary<ulong, ulong> assetValues = await this.FetchAssetValues();
            IEnumerable<Account> accounts = await this.FetchAccounts();

            AirdropUnitCollectionManager collectionManager = new AirdropUnitCollectionManager();

            Parallel.ForEach(accounts, new ParallelOptions { MaxDegreeOfParallelism = 10 }, account =>
            {
                this.AddAssetsInAccount(collectionManager, account, assetValues);
            });

            return collectionManager.GetAirdropUnitCollections();
        }

        public void AddAssetsInAccount(AirdropUnitCollectionManager collectionManager, Account account, IDictionary<ulong, ulong> assetValues)
        {
            IEnumerable<AssetHolding> assetHoldings = account.Assets;

            if (assetHoldings != null)
            {
                foreach (AssetHolding asset in assetHoldings)
                {
                    ulong sourceAssetId = asset.AssetId;
                    ulong numberOfSourceAsset = asset.Amount;

                    if (assetValues.ContainsKey(sourceAssetId) && numberOfSourceAsset > 0)
                    {
                        ulong assetValue = assetValues[sourceAssetId];
                        collectionManager.AddAirdropUnit(new AirdropUnit(
                            account.Address,
                            this.DropAssetId,
                            sourceAssetId,
                            assetValue,
                            numberOfSourceAsset: numberOfSourceAsset,
                            isMultiplied: true));
                    }
                }
            }
        }
    }
}
