using Algorand.V2.Indexer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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
            IDictionary<ulong, ulong> assetValues = await this.FetchAssetValues();
            IEnumerable<Account> accounts = await this.FetchAccounts();
            IDictionary<string, List<(ulong, ulong)>> randAccounts = await this.FetchRandAccounts();

            AirdropUnitCollectionManager collectionManager = new AirdropUnitCollectionManager();

            Parallel.ForEach(accounts, new ParallelOptions { MaxDegreeOfParallelism = 10 }, account =>
            {
                this.AddAssetsInAccount(collectionManager, account, assetValues);

                string address = account.Address;

                if (randAccounts.ContainsKey(address))
                {
                    this.AddAssetsInList(collectionManager, address, randAccounts[address], assetValues);
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
                string randEndpoint = "https://www.randswap.com/v1/secondary/get-listings-for-creator?creatorAddress=" + creatorAddress;

                string jsonResponse = await HttpClient.GetStringAsync(randEndpoint);
                Dictionary<string, dynamic> sellers = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonResponse);

                foreach (var key in sellers.Keys)
                {
                    if (key != "name" && key != "royalty" && key != "escrowAddress")
                    {
                        ulong assetId = ulong.Parse(key);
                        string walletAddress = sellers[key]["seller"];

                        if (randSellers.ContainsKey(walletAddress))
                        {
                            randSellers[key].Add((assetId, 1));
                        }
                        else
                        {
                            randSellers[key] = new List<(ulong, ulong)>() { (assetId, 1) };
                        }
                    }
                }
            }

            return randSellers;
        }
    }
}
