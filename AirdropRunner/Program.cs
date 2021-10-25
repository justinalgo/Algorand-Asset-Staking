using Airdrop;
using Algorand.V2;
using Util;
using System;
using System.Linq;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Account = Algorand.Account;
using Transaction = Algorand.Transaction;
using Algorand;
using Algorand.V2.Model;
using Algorand.Client;
using System.Collections.Generic;

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

            var mnemonic = client.GetSecret("lingLingMnemonic").Value.Value;

            AlgodApi algod = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            IndexerApi indexer = new IndexerApi(INDEXER_API_ADDR, INDEXER_API_TOKEN);
            Api api = new Api(algod, indexer);

            Account account = new Account(mnemonic);

            AirdropFactory airdropFactory = new ShrimpAirdropFactory(api);

            IDictionary<long, long> assetValues = airdropFactory.GetAssetValues();

            IEnumerable<AirdropAmount> airdropAmounts = airdropFactory.FetchAirdropAmounts(assetValues);

            foreach (AirdropAmount airdropAmount in airdropAmounts)
            {
                Console.WriteLine(airdropAmount.Wallet + " : " + airdropAmount.Amount);
                TransactionParametersResponse transactionParameters = algod.TransactionParams();

                Transaction txn = Utils.GetTransferAssetTransaction(
                        account.Address,
                        new Address(airdropAmount.Wallet),
                        airdropFactory.AssetId,
                        (ulong)airdropAmount.Amount,
                        transactionParameters
                    );
                SignedTransaction stxn = account.SignTransaction(txn);

                api.SubmitTransaction(stxn);
            }

            Console.WriteLine("Total: " + airdropAmounts.Sum(aa => aa.Amount));
            Console.WriteLine("Number of wallets: " + airdropAmounts.Count());
        }
    }
}
