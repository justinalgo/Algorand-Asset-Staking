using Algorand.V2.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Util;
using Util.Cosmos;

namespace Airdrop.AirdropFactories.Holdings
{
    public class RaptorHoldingsFactory : IHoldingsAirdropFactory
    {
        public long AssetId { get; set; }
        public long Decimals { get; set; }
        public string[] CreatorAddresses { get; set; }
        private readonly IAlgoApi api;
        private readonly ICosmos cosmos;
        private readonly HttpClient httpClient;

        public RaptorHoldingsFactory(IAlgoApi api, ICosmos cosmos, IHttpClientFactory httpClientFactory)
        {
            this.AssetId = 426980914;
            this.Decimals = 2;
            this.CreatorAddresses = new string[] { 
                "EYERHFMTLLBVVDV4RKGKSTMWU4QZ7IG5HUEXQYO3UVX4ABQ5MDNJIKN7HQ",
                "MOON3QNZT5XBPIYLMASRS5RSFRVVZF4SAMG4Q22KQFRLWKXXW2S2MY2WD4",
                "OPBIE5S3IUKLNFU6C2DK5B3FTYD7W7BMVDTNMTFFLKVQXPUUNAFBHZHVBY",
                "BEEE3WGLXN6QD62D3LVB67DF5LESMIS6FVD4QTMOV3325OM24CDQKBWI6U",
            };
            this.api = api;
            this.cosmos = cosmos;
            this.httpClient = httpClientFactory.CreateClient();
        }

        public async Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts()
        {
            IDictionary<long, long> assetValues = await this.FetchAssetValues();
            ConcurrentBag<AirdropAmount> airdropAmounts = new ConcurrentBag<AirdropAmount>();
            IEnumerable<string> walletAddresses = this.FetchWalletAddresses();
            IDictionary<string, long> randValues = await this.GetRandValues(assetValues);

            Parallel.ForEach(walletAddresses, new ParallelOptions { MaxDegreeOfParallelism = 10 }, walletAddress =>
            {
                IEnumerable<AssetHolding> assetHoldings = this.api.GetAssetsByAddress(walletAddress);
                long amount = this.GetAssetHoldingsAmount(assetHoldings, assetValues);

                amount += this.GetRandAmount(walletAddress, randValues);

                if (amount > 0)
                {
                    airdropAmounts.Add(new AirdropAmount(walletAddress, this.AssetId, amount));
                }
            });

            return airdropAmounts;
        }

        public async Task<IDictionary<long, long>> FetchAssetValues()
        {
            IEnumerable<AssetValue> values = await cosmos.GetAssetValues("RaptorCoin", "MoonDude", "Numbers", "BuzzyBees");

            Dictionary<long, long> assetValues = values.ToDictionary(av => av.AssetId, av => av.Value);

            return assetValues;
        }

        public IEnumerable<string> FetchWalletAddresses()
        {
            IEnumerable<string> walletAddresses = this.api.GetWalletAddressesWithAsset(this.AssetId);

            return walletAddresses;
        }

        public long GetAssetHoldingsAmount(IEnumerable<AssetHolding> assetHoldings, IDictionary<long, long> assetValues)
        {
            long airdropAmount = 0;

            foreach (AssetHolding miniAssetHolding in assetHoldings)
            {
                if (miniAssetHolding.AssetId.HasValue &&
                    miniAssetHolding.Amount.HasValue &&
                    miniAssetHolding.Amount > 0 &&
                    assetValues.ContainsKey(miniAssetHolding.AssetId.Value))
                {
                    airdropAmount += (long)(assetValues[miniAssetHolding.AssetId.Value] * Math.Pow(10, this.Decimals) * miniAssetHolding.Amount.Value);
                }
            }

            return airdropAmount;
        }

        private long GetRandAmount(string walletAddress, IDictionary<string, long> randValues)
        {
            if (randValues.ContainsKey(walletAddress))
            {
                return randValues[walletAddress];
            }

            return 0;
        }

        public async Task<IDictionary<string, long>> GetRandValues(IDictionary<long, long> assetValues)
        {
            Dictionary<string, long> walletValues = new Dictionary<string, long>();
            IEnumerable<(long, string)> randSellers = await GetRandSellers();

            foreach ((long, string) randSeller in randSellers)
            {
                if (walletValues.ContainsKey(randSeller.Item2) && assetValues.ContainsKey(randSeller.Item1))
                {
                    walletValues[randSeller.Item2] += (long)(assetValues[randSeller.Item1] * Math.Pow(10, this.Decimals));
                }
                else if (assetValues.ContainsKey(randSeller.Item1))
                {
                    walletValues[randSeller.Item2] = (long)(assetValues[randSeller.Item1] * Math.Pow(10, this.Decimals));
                }
            }

            return walletValues;
        }

        private async Task<IEnumerable<(long, string)>> GetRandSellers()
        {
            List<(long, string)> randSellers = new List<(long, string)>();

            foreach (string creatorAddress in this.CreatorAddresses)
            {
                string randEndpoint = "https://www.randswap.com/v1/secondary/get-listings-for-creator?creatorAddress=" + creatorAddress;

                string jsonResponse = await httpClient.GetStringAsync(randEndpoint);
                Dictionary<string, dynamic> sellers = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonResponse);

                foreach (var assetId in sellers.Keys)
                {
                    if (assetId != "name" && assetId != "royalty" && assetId != "escrowAddress")
                    {
                        randSellers.Add((long.Parse(assetId), sellers[assetId]["seller"]));
                    }
                }
            }

            return randSellers;
        }
    }
}
