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
    public class RaptorHoldingsFactory : HoldingsAirdropFactory
    {
        private readonly IIndexerUtils indexerUtils;
        private readonly ICosmos cosmos;
        private readonly HttpClient httpClient;

        public RaptorHoldingsFactory(IIndexerUtils indexerUtils, ICosmos cosmos, IHttpClientFactory httpClientFactory)
        {
            this.AssetId = 426980914;
            this.Decimals = 2;
            this.CreatorAddresses = new string[] { 
                "EYERHFMTLLBVVDV4RKGKSTMWU4QZ7IG5HUEXQYO3UVX4ABQ5MDNJIKN7HQ",
                "MOON3QNZT5XBPIYLMASRS5RSFRVVZF4SAMG4Q22KQFRLWKXXW2S2MY2WD4",
                "OPBIE5S3IUKLNFU6C2DK5B3FTYD7W7BMVDTNMTFFLKVQXPUUNAFBHZHVBY",
                "BEEE3WGLXN6QD62D3LVB67DF5LESMIS6FVD4QTMOV3325OM24CDQKBWI6U",
            };
            this.indexerUtils = indexerUtils;
            this.cosmos = cosmos;
            this.httpClient = httpClientFactory.CreateClient();
        }

        public override async Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts()
        {
            IDictionary<ulong, ulong> assetValues = await this.FetchAssetValues();
            ConcurrentBag<AirdropAmount> airdropAmounts = new ConcurrentBag<AirdropAmount>();
            IEnumerable<string> walletAddresses = await this.FetchWalletAddresses();
            IDictionary<string, ulong> randValues = await this.GetRandValues(assetValues);

            Parallel.ForEach(walletAddresses, new ParallelOptions { MaxDegreeOfParallelism = 10 }, walletAddress =>
            {
                Account account = this.indexerUtils.GetAccount(walletAddress).Result;
                IEnumerable<AssetHolding> assetHoldings = account.Assets;
                ulong amount = this.GetAssetHoldingsAmount(assetHoldings, assetValues);

                amount += this.GetRandAmount(walletAddress, randValues);

                if (amount > 0)
                {
                    airdropAmounts.Add(new AirdropAmount(walletAddress, this.AssetId, amount));
                }
            });

            return airdropAmounts;
        }

        public override async Task<IDictionary<ulong, ulong>> FetchAssetValues()
        {
            IEnumerable<AssetValue> values = await cosmos.GetAssetValues("RaptorCoin", "MoonDude", "Numbers", "BuzzyBees");

            Dictionary<ulong, ulong> assetValues = values.ToDictionary(av => av.AssetId, av => av.Value);

            return assetValues;
        }

        public override async Task<IEnumerable<string>> FetchWalletAddresses()
        {
            IEnumerable<string> walletAddresses = await this.indexerUtils.GetWalletAddresses(this.AssetId);

            return walletAddresses;
        }

        private ulong GetRandAmount(string walletAddress, IDictionary<string, ulong> randValues)
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
