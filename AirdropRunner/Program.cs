using Airdrop;
using Algorand.V2;
using Util;
using System;
using System.Linq;
using System.Collections.Generic;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;

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

            var kvUri = "https://cavernavault.vault.azure.net";
            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential(true));

            var mnemonic = client.GetSecret("airdropMnemonic");

            Console.WriteLine(mnemonic.Value.Value);

            AlgodApi algod = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            IndexerApi indexer = new IndexerApi(INDEXER_API_ADDR, INDEXER_API_TOKEN);
            Api api = new Api(algod, indexer);

            AirdropFactory airdropFactory = new CryptoBunnyAirdropFactory(api);

            /*IDictionary<long, long> assetValues = airdropFactory.GetAssetValues();

            IEnumerable<AirdropAmount> airdropAmounts = airdropFactory.FetchAirdropAmounts(assetValues);

            foreach (AirdropAmount airdropAmount in airdropAmounts)
            {
                Console.WriteLine(airdropAmount.Wallet + " : " + airdropAmount.Amount);
            }

            Console.WriteLine("Total: " + airdropAmounts.Sum(a => a.Amount));

            IEnumerable<RetrievedAsset> retrievedAssets = airdropFactory.CheckAssets();

            foreach (RetrievedAsset retrievedAsset in retrievedAssets.OrderBy(ra => ra.UnitName))
            {
                Console.WriteLine(retrievedAsset.UnitName + " : " + retrievedAsset.AssetId + " : " + retrievedAsset.Name);
            }*/
        }
    }
}
