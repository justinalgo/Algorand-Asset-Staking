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

namespace Airdrop
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

            int AlcheCoinId = 310014962;
            int AlcheCoinStakeId = 320570576;

            IEnumerable<string> dropAddresses = api.GetWalletAddressesWithAsset(AlcheCoinId, AlcheCoinStakeId);

            List<AlcheCoinAsset> alchemonValues = JsonConvert.DeserializeObject<List<AlcheCoinAsset>>(File.ReadAllText("C:/Users/ParkG/source/repos/Airdrop/AlcheCoinAirdrop/AlchemonValues.json"));

            Dictionary<long, AlcheCoinAsset> assetValues = alchemonValues.ToDictionary(av => av.AssetId);

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
                    /*var txn = Utils.GetTransferAssetTransaction(wallet.Address, new Address(account.Address), AlcheCoinId, (ulong)amount, transParams,
                        message: "Enjoy your AlcheCoin airdrop! See you next week! :)");
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
                    numReceived++;*/
                }
            }

            Console.WriteLine("Total: " + total);
            Console.WriteLine("Number received: " + numReceived);
        }

        static long GetAirdropAmount(Account account, Dictionary<long, AlcheCoinAsset> assetValues)
        {
            long baseAmount = 0;
            int numberOfAssets = 0;

            foreach (AssetHolding miniAssetHolding in account.Assets)
            {
                if (miniAssetHolding.AssetId.HasValue && 
                    miniAssetHolding.Amount > 0 &&
                    assetValues.ContainsKey(miniAssetHolding.AssetId.Value))
                {
                    numberOfAssets++;

                    if (assetValues[miniAssetHolding.AssetId.Value].Value > baseAmount)
                    {
                        baseAmount = assetValues[miniAssetHolding.AssetId.Value].Value;
                    }
                }
            }

            return baseAmount + (numberOfAssets > 0 ? 2 * (numberOfAssets - 1) : 0);
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
