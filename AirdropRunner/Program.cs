using Airdrop;
using Algorand.V2;
using Api;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace AirdropRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            string ALGOD_API_ADDR = "http://cavernatech.eastus.cloudapp.azure.com:8080";
            string ALGOD_API_TOKEN = "0c9add6d713cefb54c8106282e12b700735afef18b073f91f86e9e2142df695d";

            string INDEXER_API_ADDR = "https://mainnet-algorand.api.purestake.io/idx2";
            string INDEXER_API_TOKEN = "HFoxXc2sQf7ut4bAVmfg0adKQ6RRqTCi6nEg0YIs";

            AlgodApi algod = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            IndexerApi indexer = new IndexerApi(INDEXER_API_ADDR, INDEXER_API_TOKEN);
            ApiUtil api = new ApiUtil(algod, indexer);

            AirdropFactory alchemonAirdropFactory = new AlchemonAirdropFactory(api);

            List<AlcheCoinAsset> alchemonValues = JsonConvert.DeserializeObject<List<AlcheCoinAsset>>(File.ReadAllText("C:/Users/ParkG/source/repos/Airdrop/AlcheCoinAirdrop/AlchemonValues.json"));

            Dictionary<long, long> assetValues = alchemonValues.ToDictionary(av => av.AssetId, av => av.Value);

            IEnumerable<AirdropAmount> airdropAmounts = alchemonAirdropFactory.FetchAirdropAmounts(assetValues);

            foreach (AirdropAmount airdropAmount in airdropAmounts)
            {
                Console.WriteLine(airdropAmount.Wallet + " : " + airdropAmount.Amount);
            }

            Console.WriteLine("Total: " + airdropAmounts.Sum(a => a.Amount));
        }
    }

    class AlcheCoinAsset
    {
        public string Name { get; set; }
        public string UnitName { get; set; }
        public long AssetId { get; set; }
        public long Value { get; set; }
    }
}
