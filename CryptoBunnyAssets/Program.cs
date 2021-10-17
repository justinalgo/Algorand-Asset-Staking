using Algorand.V2;
using Algorand.V2.Model;
using Api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace CryptoBunnyAssets
{
    class Program
    {
        static void Main(string[] args)
        {
            string ALGOD_API_ADDR = "http://cavernatech.eastus.cloudapp.azure.com:8080";
            string ALGOD_API_TOKEN = "0c9add6d713cefb54c8106282e12b700735afef18b073f91f86e9e2142df695d";

            string INDEXER_API_ADDR = "https://mainnet-algorand.api.purestake.io/idx2";
            string INDEXER_API_TOKEN = "HFoxXc2sQf7ut4bAVmfg0adKQ6RRqTCi6nEg0YIs";

            string mnemonic = Environment.GetEnvironmentVariable("mnemonic");

            AlgodApi algod = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            IndexerApi indexer = new IndexerApi(INDEXER_API_ADDR, INDEXER_API_TOKEN);
            ApiUtil api = new ApiUtil(algod, indexer);

            Account account = api.GetAccountByAddress("BNYSETPFTL2657B5RCSW64A3M766GYBVRV5ALOM7F7LIRUZKBEOGF6YSO4");

            List<long> assets = account.Assets.ConvertAll<long>(ah => ah.AssetId.Value);
            List<CryptoBunnyLegendary> cbAssets = new List<CryptoBunnyLegendary>();

            foreach (long asset in assets)
            {
                Asset assetResponse = api.GetAssetById(asset);

                if (assetResponse != null)
                {
                    Console.WriteLine(assetResponse.Params.UnitName);

                    if (assetResponse.Params.UnitName.StartsWith("BNYL") || assetResponse.Params.Name.Contains("BNYO"))
                    {
                        CryptoBunnyLegendary aca = new CryptoBunnyLegendary(
                            assetResponse.Params.Name,
                            assetResponse.Params.UnitName,
                            asset
                        );

                        cbAssets.Add(aca);
                    }
                }

                Thread.Sleep(1500);
            }

            using (StreamWriter file = File.CreateText("./assets.json"))
            {
                JsonSerializer serializer = new JsonSerializer();

                serializer.Serialize(file, cbAssets.OrderBy(a => a.UnitName));
            }
        }
    }

    class CryptoBunnyLegendary {
        public string Name { get; set; }
        public string UnitName { get; set; }
        public long AssetId { get; set; }
        public int? Value { get; set; }

        public CryptoBunnyLegendary(string name, string unitName, long assetId)
        {
            Name = name;
            UnitName = unitName;
            AssetId = assetId;
            Value = 1;
        }
    }
}
