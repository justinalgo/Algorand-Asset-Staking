using System;
using System.Linq;
using Algorand;
using Algorand.V2;
using Algorand.Client;
using Algorand.V2.Model;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Account = Algorand.V2.Model.Account;
using Wallet = Algorand.Account;
using AssetHolding = Algorand.V2.Model.AssetHolding;
using Api;

namespace CryptoBunnyAirdrop
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

            Console.WriteLine("Getting addresses...");

            int cryptoBunnyCarrotId = 329532956;

            IEnumerable<string> dropAddresses = api.GetWalletAddressesWithAsset(cryptoBunnyCarrotId);

            List<CryptoBunnyLegendary> cryptoBunnies = JsonConvert.DeserializeObject<List<CryptoBunnyLegendary>>(File.ReadAllText("C:/Users/ParkG/source/repos/Airdrop/CryptoBunnyAirdrop/assets.json"));

            Dictionary<long, CryptoBunnyLegendary> assetValues = cryptoBunnies.ToDictionary(cb => cb.AssetId);

            Wallet wallet = new Wallet(mnemonic);

            TransactionParametersResponse transParams = algod.TransactionParams();

            long total = 0;
            int numReceived = 0;

            foreach (string address in dropAddresses)
            {
                Account account = api.GetAccountByAddress(address);
                long amount = GetAirdropAmount(account, assetValues);

                Console.WriteLine(account.Address + ": " + amount);

                total += amount;

                if (amount > 0)
                {
                    var txn = Utils.GetTransferAssetTransaction(wallet.Address, new Address(account.Address), cryptoBunnyCarrotId, (ulong)amount, transParams);
                    var signedTx = wallet.SignTransaction(txn);

                    try
                    {
                        api.SubmitTransaction(signedTx);
                    }
                    catch (ApiException e)
                    {
                        // This is generally expected, but should give us an informative error message.
                        Console.WriteLine("Exception when calling algod#rawTransaction: " + e.Message);
                    }
                    numReceived++;
                }
            }

            Console.WriteLine("Total: " + total);
            Console.WriteLine("Number received: " + numReceived);
        }

        static long GetAirdropAmount(Account account, Dictionary<long, CryptoBunnyLegendary> assetValues)
        {
            long airdropAmount = 0;

            foreach (AssetHolding miniAssetHolding in account.Assets)
            {
                if (miniAssetHolding.AssetId.HasValue &&
                    miniAssetHolding.Amount.HasValue &&
                    miniAssetHolding.Amount > 0 &&
                    assetValues.ContainsKey(miniAssetHolding.AssetId.Value))
                {
                    airdropAmount += (long)miniAssetHolding.Amount.Value;
                }
            }

            return airdropAmount;
        }
    }

    class CryptoBunnyLegendary
    {
        public string Name { get; set; }
        public string UnitName { get; set; }
        public long AssetId { get; set; }
        public int? Value { get; set; }
    }
}
