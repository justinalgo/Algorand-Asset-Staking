using System;
using System.Linq;
using Algorand;
using Algorand.V2;
using Algorand.Client;
using Algorand.V2.Model;
using Account = Algorand.Account;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace Airdrop
{
    class Program
    {
        static void Main(string[] args)
        {
            string ALGOD_API_ADDR = "http://localhost:8111";
            string ALGOD_API_TOKEN = "4430a9d11f819af4ed7bc1cbba3545d06230ed9499918902c40d4caaea7d4540";

            string INDEXER_API_ADDR = "https://mainnet-algorand.api.purestake.io/idx2";
            string INDEXER_API_TOKEN = "HFoxXc2sQf7ut4bAVmfg0adKQ6RRqTCi6nEg0YIs";

            AlgodApi algod = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            IndexerApi indexer = new IndexerApi(INDEXER_API_ADDR, INDEXER_API_TOKEN);
            ApiUtil api = new ApiUtil(algod, indexer);
            
            //IEnumerable<string> dropAddresses = api.GetWalletAddressesWithAsset(310014962, 320570576);

            List<AssetValue> alchemonValues = JsonConvert.DeserializeObject<List<AssetValue>>(File.ReadAllText("C:/Users/ParkG/source/repos/Airdrop/Airdrop/AlchemonValues.json"));

            Dictionary<long, AssetValue> assetValues = alchemonValues.ToDictionary(av => av.Id);



            Console.WriteLine(val);
        }
    }

    public class AssetValue
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("value")]
        public long Value { get; set; }

        [JsonProperty("decimals")]
        public long Decimals { get; set; }
    }
}
