using Algorand.V2.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Util;
using Util.Cosmos;

namespace Airdrop.AirdropFactories.Holdings
{
    public class RaptorHoldingsFactory : IHoldingsAirdropFactory
    {
        public long AssetId { get; set; }
        public long Decimals { get; set; }
        private readonly IAlgoApi api;
        private readonly ICosmos cosmos;
        private readonly HttpClient client;

        public RaptorHoldingsFactory(IAlgoApi api, ICosmos cosmos)
        {
            this.AssetId = 426980914;
            this.Decimals = 2;
            this.api = api;
            this.cosmos = cosmos;
            this.client = new HttpClient();
        }

        public async Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts()
        {
            IDictionary<long, long> assetValues = await this.FetchAssetValues();
            ConcurrentBag<AirdropAmount> airdropAmounts = new ConcurrentBag<AirdropAmount>();
            IEnumerable<string> walletAddresses = this.FetchWalletAddresses();
            //IDictionary<string, long> randValues = await this.GetRandValues(assetValues);

            Parallel.ForEach(walletAddresses, walletAddress =>
            {
                IEnumerable<AssetHolding> assetHoldings = this.api.GetAssetsByAddress(walletAddress);
                long amount = this.GetAssetHoldingsAmount(assetHoldings, assetValues);

                //amount += this.GetRandAmount(walletAddress, randValues);

                if (amount > 0)
                {
                    airdropAmounts.Add(new AirdropAmount(walletAddress, this.AssetId, amount));
                }
            });

            return airdropAmounts;
        }

        public async Task<IDictionary<long, long>> FetchAssetValues()
        {
            IEnumerable<AssetValue> values = await cosmos.GetAssetValues("RaptorCoin");

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

        public long GetRandAmount(string walletAddress, IDictionary<string, long> randValues)
        {
            if (randValues.ContainsKey(walletAddress))
            {
                return randValues[walletAddress];
            }

            return 0;
        }

        private async Task<Dictionary<string, long>> GetRandValues(IDictionary<long, long> assetValues)
        {
            Dictionary<string, long> walletValues = new Dictionary<string, long>();
            IEnumerable<(long, string)> randSellers = await GetRandSellers();

            foreach ((long, string) randSeller in randSellers)
            {
                if (walletValues.ContainsKey(randSeller.Item2) && assetValues.ContainsKey(randSeller.Item1))
                {
                    walletValues[randSeller.Item2] += assetValues[randSeller.Item1];
                }
                else if (assetValues.ContainsKey(randSeller.Item1))
                {
                    walletValues[randSeller.Item2] = assetValues[randSeller.Item1];
                }
            }

            return walletValues;
        }

        private async Task<IEnumerable<(long, string)>> GetRandSellers()
        {
            string randEndpoint = "https://www.randswap.com/v1/secondary/get-listings-for-creator?creatorAddress=NV7D4EFKO5FRXEHRVMEP3LDG6IACFQVJXYYG6KJAGXW2JRBKW3Y7UNQE2Y";

            string jsonResponse = await client.GetStringAsync(randEndpoint);
            Dictionary<string, dynamic> sellers = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonResponse);

            List<(long, string)> randSellers = new List<(long, string)>();

            foreach (var assetId in sellers.Keys)
            {
                if (assetId != "name" && assetId != "royalty" && assetId != "escrowAddress")
                {
                    randSellers.Add((long.Parse(assetId), sellers[assetId]["seller"]));
                }
            }

            return randSellers;
        }
    }
}
