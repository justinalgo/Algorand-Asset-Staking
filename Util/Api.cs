using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Algorand;
using Algorand.V2;
using Algorand.V2.Model;
using System.Threading;
using Algorand.Client;
using Microsoft.Extensions.Configuration;

namespace Util
{
    public class Api : IApi
    {
        private readonly ILogger<Api> log;
        private readonly IConfiguration config;
        private readonly AlgodApi algod;
        private readonly IndexerApi indexer;

        public Api(ILogger<Api> log, IConfiguration config)
        {
            this.log = log;
            this.config = config;
            this.algod = new AlgodApi(config["Endpoints:Algod"], config["AlgodToken"]);
            this.indexer = new IndexerApi(config["Endpoints:Indexer"], config["IndexerToken"]);
        }

        public IEnumerable<string> GetWalletAddressesWithAsset(long assetId)
        {
            AssetBalancesResponse assetInfo = this.GetAssetBalances(assetId);

            List<string> walletAddresses = new List<string>();

            foreach (MiniAssetHolding account in assetInfo.Balances)
            {
                walletAddresses.Add(account.Address);
            }

            while (assetInfo.NextToken != null)
            {
                assetInfo = this.GetAssetBalances(assetId, next: assetInfo.NextToken);

                foreach (MiniAssetHolding account in assetInfo.Balances)
                {
                    walletAddresses.Add(account.Address);
                }
            }

            return walletAddresses;
        }

        public IEnumerable<string> GetWalletAddressesWithAsset(long assetId, params long[] assetIds)
        {
            IEnumerable<string> addresses = GetWalletAddressesWithAsset(assetId);

            foreach (int id in assetIds)
            {
                IEnumerable<string> intersectAddresses = GetWalletAddressesWithAsset(id);
                addresses = addresses.Intersect(intersectAddresses);
            }

            return addresses.OrderBy(a => a);
        }

        public PendingTransactionResponse SubmitTransaction(SignedTransaction signedTxn)
        {
            PostTransactionsResponse id = Utils.SubmitTransaction(algod, signedTxn);
            PendingTransactionResponse resp = Utils.WaitTransactionToComplete(algod, id.TxId);
            return resp;
        }

        public Asset GetAssetById(long assetId)
        {
            Asset asset = null;
            int maxRetries = 5;
            int currentRetry = 0;

            while (asset == null && currentRetry < maxRetries)
            {
                try
                {
                    asset = indexer.LookupAssetByID(assetId).Asset;
                }
                catch (ApiException apiException)
                {
                    if (apiException.ErrorCode == 404)
                    {
                        return null;
                    }
                    else if (apiException.ErrorCode == 429)
                    {
                        currentRetry++;

                        Thread.Sleep(1500); //PureStake Sleep
                    }
                    else
                    {
                        throw apiException;
                    }
                }
            }

            if (currentRetry > maxRetries - 1)
            {
                throw new Exception("GetAssetBalances ran out of retries");
            }

            return asset;
        }

        public IEnumerable<Asset> GetAssetById(IEnumerable<long> assetIds)
        {
            List<Asset> retrievedAssets = new List<Asset>();

            foreach (long assetId in assetIds)
            {
                Asset asset = this.GetAssetById(assetId);

                if (asset != null)
                {
                    retrievedAssets.Add(asset);
                }
            }

            return retrievedAssets;
        }

        public AssetBalancesResponse GetAssetBalances(long assetId, string next = null)
        {
            AssetBalancesResponse assetInfo = null;
            int maxRetries = 5;
            int currentRetry = 0;

            while (assetInfo == null && currentRetry < maxRetries)
            {
                try
                {
                    if (next == null)
                    {
                        assetInfo = indexer.LookupAssetBalances(assetId);
                    }
                    else
                    {
                        assetInfo = indexer.LookupAssetBalances(assetId, next: next);
                    }
                }
                catch (ApiException apiException)
                {
                    if (apiException.ErrorCode == 429)
                    {
                        currentRetry++;

                        Thread.Sleep(1500); //PureStake Sleep
                    }
                    else
                    {
                        throw apiException;
                    }
                }
            }

            if (currentRetry > maxRetries - 1)
            {
                throw new Exception("GetAssetBalances ran out of retries");
            }

            return assetInfo;
        }

        public long GetAssetDecimals(int assetId)
        {
            return algod.GetAssetByID(assetId).Params.Decimals.Value;
        }

        public Algorand.V2.Model.Account GetAccountByAddress(string walletAddress)
        {
            return algod.AccountInformation(walletAddress);
        }

        public IEnumerable<AssetHolding> GetAssetsByAddress(string walletAddress)
        {
            return this.GetAccountByAddress(walletAddress).Assets;
        }

        public TransactionParametersResponse GetTransactionParams()
        {
            return this.algod.TransactionParams();
        }
    }
}
