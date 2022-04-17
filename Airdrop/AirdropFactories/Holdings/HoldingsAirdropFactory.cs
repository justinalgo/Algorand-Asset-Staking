using Algorand.V2.Algod.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Holdings
{
    public abstract class HoldingsAirdropFactory
    {
        public IAlgodUtils AlgodUtils { get; }
        public IIndexerUtils IndexerUtils { get; }
        public ulong DropAssetId { get; set; }
        public ulong Decimals { get; set; }
        public string[] CreatorAddresses { get; set; }
        public string[] RevokedAddresses { get; set; }
        public ulong[] RevokedAssets { get; set; }
        public ulong AssetValue { get; set; }

        public HoldingsAirdropFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils)
        {
            this.IndexerUtils = indexerUtils;
            this.AlgodUtils = algodUtils;
        }

        public virtual async Task<IDictionary<ulong, ulong>> FetchAssetValues()
        {
            Dictionary<ulong, ulong> assetValues = new Dictionary<ulong, ulong>();

            foreach (string creatorAddress in this.CreatorAddresses)
            {
                var assets = await this.IndexerUtils.GetCreatedAssets(creatorAddress);

                if (this.RevokedAddresses != null)
                {
                    foreach (var asset in assets)
                    {
                        if (!this.RevokedAssets.Contains(asset.Index))
                        {
                            assetValues.Add(asset.Index, this.AssetValue);
                        }
                    }
                }
                else
                {
                    foreach (var asset in assets)
                    {
                        assetValues.Add(asset.Index, this.AssetValue);
                    }
                }
            }

            return assetValues;
        }

        public virtual async Task<IEnumerable<Account>> FetchAccounts()
        {
            IEnumerable<string> addresses = await this.IndexerUtils.GetWalletAddresses(this.DropAssetId);

            Console.WriteLine(addresses.Count());

            ConcurrentBag<Account> accounts = new ConcurrentBag<Account>();

            Parallel.ForEach<string>(addresses, new ParallelOptions { MaxDegreeOfParallelism = 10 }, address =>
            {
                var account = this.AlgodUtils.GetAccount(address).Result;
                accounts.Add(account);
            });

            if (this.RevokedAddresses != null)
            {
                return accounts.Where(a => !this.RevokedAddresses.Contains(a.Address));
            }

            return accounts;
        }

        public virtual async Task<IEnumerable<AirdropUnitCollection>> FetchAirdropUnitCollections()
        {
            IDictionary<ulong, ulong> assetValues = await FetchAssetValues();
            IEnumerable<Account> accounts = await FetchAccounts();

            AirdropUnitCollectionManager collectionManager = new AirdropUnitCollectionManager();

            Parallel.ForEach(accounts, new ParallelOptions { MaxDegreeOfParallelism = 10 }, account =>
            {
                AddAssetsInAccount(collectionManager, account, assetValues);
            });

            return collectionManager.GetAirdropUnitCollections();
        }

        public virtual void AddAssetsInAccount(AirdropUnitCollectionManager collectionManager, Account account, IDictionary<ulong, ulong> assetValues)
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
