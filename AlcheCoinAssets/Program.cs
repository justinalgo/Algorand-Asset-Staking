using Algorand.V2;
using Algorand.V2.Model;
using Api;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using Newtonsoft.Json;

namespace AlcheCoinAssets
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

            Account account = api.GetAccountByAddress("OJGTHEJ2O5NXN7FVXDZZEEJTUEQHHCIYIE5MWY6BEFVVLZ2KANJODBOKGA");

            List<long> assets = account.Assets.ConvertAll<long>(ah => ah.AssetId.Value);
            List<AlcheCoinAsset> alchAssets = new List<AlcheCoinAsset>();

            foreach (long asset in assets)
            {
                Asset assetResponse = api.GetAssetById(asset);

                if (assetResponse != null)
                {
                    Console.WriteLine(assetResponse.Params.UnitName);

                    if (assetResponse.Params.UnitName.StartsWith("ALCH"))
                    {
                        AlcheCoinAsset aca = new AlcheCoinAsset(
                            assetResponse.Params.Name,
                            assetResponse.Params.UnitName,
                            asset,
                            null
                        );

                        alchAssets.Add(aca);
                    }
                }

                Thread.Sleep(1500);
            }
            
            using (StreamWriter file = File.CreateText("./assets.json"))
            {
                JsonSerializer serializer = new JsonSerializer();

                serializer.Serialize(file, alchAssets.OrderBy(a => a.UnitName));
            }
        }
    }

    class AlcheCoinAsset
    {
        public string Name { get; set; }
        public string UnitName { get; set; }
        public long AssetId { get; set; }
        public int? Value { get; set; }

        public AlcheCoinAsset(string Name, string UnitName, long AssetId, int? Value)
        {
            this.Name = Name;
            this.UnitName = UnitName;
            this.AssetId = AssetId;
            this.Value = Value;
        }
    }
}
