using Algorand.V2.Indexer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Util;
using Util.Cosmos;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Holdings
{
    public class NanaHoldingsFactory : HoldingsAirdropFactory
    {
        private readonly IIndexerUtils indexerUtils;
        private readonly ICosmos cosmos;
        private readonly HttpClient httpClient;

        public NanaHoldingsFactory(IIndexerUtils indexerUtils, ICosmos cosmos, IHttpClientFactory httpClientFactory)
        {
            this.AssetId = 418706707;
            this.Decimals = 0;
            this.CreatorAddresses = new string[] { "NV7D4EFKO5FRXEHRVMEP3LDG6IACFQVJXYYG6KJAGXW2JRBKW3Y7UNQE2Y" };
            this.indexerUtils = indexerUtils;
            this.cosmos = cosmos;
            this.httpClient = httpClientFactory.CreateClient();
        }

        public override async Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts()
        {
            IDictionary<ulong, ulong> assetValues = await this.FetchAssetValues();
            ConcurrentBag<AirdropAmount> airdropAmounts = new ConcurrentBag<AirdropAmount>();
            IEnumerable<Account> accounts = await this.FetchAccounts();
            IDictionary<string, ulong> randValues = await this.GetRandValues(assetValues);

            Parallel.ForEach(accounts, new ParallelOptions { MaxDegreeOfParallelism = 10 }, account =>
            {
                IEnumerable<AssetHolding> assetHoldings = account.Assets;
                ulong amount = this.GetAssetHoldingsAmount(assetHoldings, assetValues);

                amount += this.GetRandAmount(account.Address, randValues);

                if (amount > 0)
                {
                    airdropAmounts.Add(new AirdropAmount(account.Address, this.AssetId, amount));
                }
            });

            return airdropAmounts;
        }

        public override async Task<IDictionary<ulong, ulong>> FetchAssetValues()
        {
            IEnumerable<AssetValue> values = await cosmos.GetAssetValues("NanaCoin");

            Dictionary<ulong, ulong> assetValues = values.ToDictionary(av => av.AssetId, av => av.Value);

            return assetValues;
        }

        public override async Task<IEnumerable<Account>> FetchAccounts()
        {
            IEnumerable<Account> accounts = await this.indexerUtils.GetAccounts(this.AssetId);

            return accounts;
        }

        public ulong GetRandAmount(string walletAddress, IDictionary<string, ulong> randValues)
        {
            if (randValues.ContainsKey(walletAddress))
            {
                return randValues[walletAddress];
            }

            return 0;
        }

        public async Task<IDictionary<string, ulong>> GetRandValues(IDictionary<ulong, ulong> assetValues)
        {
            Dictionary<string, ulong> walletValues = new Dictionary<string, ulong>();
            IEnumerable<(ulong, string)> randSellers = await GetRandSellers();

            foreach ((ulong, string) randSeller in randSellers)
            {
                if (walletValues.ContainsKey(randSeller.Item2) && assetValues.ContainsKey(randSeller.Item1))
                {
                    walletValues[randSeller.Item2] += (ulong)(assetValues[randSeller.Item1] * Math.Pow(10, this.Decimals));
                }
                else if (assetValues.ContainsKey(randSeller.Item1))
                {
                    walletValues[randSeller.Item2] = (ulong)(assetValues[randSeller.Item1] * Math.Pow(10, this.Decimals));
                }
            }

            return walletValues;
        }

        private async Task<IEnumerable<(ulong, string)>> GetRandSellers()
        {
            List<(ulong, string)> randSellers = new List<(ulong, string)>();

            foreach (string creatorAddress in this.CreatorAddresses)
            {
                string randEndpoint = "https://www.randswap.com/v1/secondary/get-listings-for-creator?creatorAddress=" + creatorAddress;

                string jsonResponse = await httpClient.GetStringAsync(randEndpoint);
                Dictionary<string, dynamic> sellers = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonResponse);

                foreach (var assetId in sellers.Keys)
                {
                    if (assetId != "name" && assetId != "royalty" && assetId != "escrowAddress")
                    {
                        randSellers.Add((ulong.Parse(assetId), sellers[assetId]["seller"]));
                    }
                }
            }

            return randSellers;
        }
    }
}
