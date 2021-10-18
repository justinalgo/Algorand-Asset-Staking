using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Algorand;
using Algorand.V2;
using Algorand.V2.Model;
using System.Threading;
using Algorand.Client;

namespace Api
{
    public class ApiUtil : IApiUtil
    {
        private readonly ILogger<ApiUtil> _log;
        private readonly AlgodApi _algod;
        private readonly IndexerApi _indexer;

        public ApiUtil(ILogger<ApiUtil> log, AlgodApi algod, IndexerApi indexer)
        {
            this._log = log;
            this._algod = algod;
            this._indexer = indexer;
        }
        public ApiUtil(AlgodApi algod, IndexerApi indexer)
        {
            this._algod = algod;
            this._indexer = indexer;
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

        private PendingTransactionResponse SubmitTransaction(SignedTransaction signedTxn)
        {
            PostTransactionsResponse id = Utils.SubmitTransaction(_algod, signedTxn);
            Console.WriteLine("Successfully sent tx with id: " + id.TxId);
            PendingTransactionResponse resp = Utils.WaitTransactionToComplete(_algod, id.TxId);
            Console.WriteLine("Confirmed Round is: " + resp.ConfirmedRound);
            return resp;
        }

        public void SubmitTransactions(IEnumerable<SignedTransaction> signedTxns)
        {
            foreach (SignedTransaction signedTxn in signedTxns)
            {
                try
                {
                    this.SubmitTransaction(signedTxn);
                }
                catch (ApiException e)
                {
                    // This is generally expected, but should give us an informative error message.
                    Console.WriteLine("Exception when calling algod#rawTransaction: " + e.Message);
                }
            }
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
                    asset = _indexer.LookupAssetByID(assetId).Asset;
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
                        assetInfo = _indexer.LookupAssetBalances(assetId);
                    }
                    else
                    {
                        assetInfo = _indexer.LookupAssetBalances(assetId, next: next);
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
            return _algod.GetAssetByID(assetId).Params.Decimals.Value;
        }

        public Algorand.V2.Model.Account GetAccountByAddress(string walletAddress)
        {
            return _algod.AccountInformation(walletAddress);
        }

        public IEnumerable<AssetHolding> GetAssetsByAddress(string walletAddress)
        {
            return this.GetAccountByAddress(walletAddress).Assets;
        }
    }
}
