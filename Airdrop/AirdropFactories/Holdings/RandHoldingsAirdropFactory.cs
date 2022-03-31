using Algorand.V2.Indexer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Airdrop.AirdropFactories.Holdings
{
    public abstract class RandHoldingsAirdropFactory : HoldingsAirdropFactory
    {
        public HttpClient HttpClient { get; }

        public RandHoldingsAirdropFactory(HttpClient httpClient)
        {
            this.HttpClient = httpClient;
        }

        public new async Task<IEnumerable<AirdropUnitCollection>> FetchAirdropUnitCollections()
        {
            IDictionary<ulong, ulong> assetValues = await FetchAssetValues();
            IEnumerable<Account> accounts = await FetchAccounts();
            IDictionary<string, List<(ulong, ulong)>> randAccounts = await FetchRandAccounts();

            AirdropUnitCollectionManager collectionManager = new AirdropUnitCollectionManager();

            Parallel.ForEach(accounts, new ParallelOptions { MaxDegreeOfParallelism = 10 }, account =>
            {
                AddAssetsInAccount(collectionManager, account, assetValues);

                string address = account.Address;

                if (randAccounts.ContainsKey(address))
                {
                    AddAssetsInList(collectionManager, address, randAccounts[address], assetValues);
                }
            });

            return collectionManager.GetAirdropUnitCollections();
        }

        public void AddAssetsInList(AirdropUnitCollectionManager collectionManager, string address, IEnumerable<(ulong, ulong)> sourceAssets, IDictionary<ulong, ulong> assetValues)
        {
            if (sourceAssets != null)
            {
                foreach ((ulong, ulong) sourceAssetInfo in sourceAssets)
                {
                    ulong sourceAssetId = sourceAssetInfo.Item1;
                    ulong numberOfSourceAsset = sourceAssetInfo.Item2;

                    if (assetValues.ContainsKey(sourceAssetId))
                    {
                        ulong sourceAssetValue = assetValues[sourceAssetId];
                        collectionManager.AddAirdropUnit(new AirdropUnit(
                            address,
                            this.DropAssetId,
                            sourceAssetId,
                            sourceAssetValue,
                            numberOfSourceAsset: numberOfSourceAsset,
                            isMultiplied: true));
                    }
                }
            }
        }

        public async Task<Dictionary<string, List<(ulong, ulong)>>> FetchRandAccounts()
        {
            Dictionary<string, List<(ulong, ulong)>> randSellers = new Dictionary<string, List<(ulong, ulong)>>();

            foreach (string creatorAddress in this.CreatorAddresses)
            {
                string randEndpoint = "https://www.randswap.com/v1/listings/creator/" + creatorAddress;

                string jsonResponse = await HttpClient.GetStringAsync(randEndpoint);
                List<RandListing> listings = JsonConvert.DeserializeObject<List<RandListing>>(jsonResponse);

                foreach (RandListing listing in listings)
                {
                    if (randSellers.ContainsKey(listing.SellerAddress))
                    {
                        randSellers[listing.SellerAddress].Add((listing.AssetId, 1));
                    }
                    else
                    {
                        randSellers[listing.SellerAddress] = new List<(ulong, ulong)>() { (listing.AssetId, 1) };
                    }
                }
            }

            return randSellers;
        }
    }

    class RandListing
    {
        [JsonProperty("assetId")]
        public ulong AssetId { get; set; }
        [JsonProperty("sellerAddress")]
        public string SellerAddress { get; set; }
    }
}
