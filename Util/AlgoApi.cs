﻿using Algorand;
using Algorand.Client;
using Algorand.V2;
using Algorand.V2.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Util.Cosmos;
using Transaction = Algorand.V2.Model.Transaction;

namespace Util
{
    public class AlgoApi : IAlgoApi
    {
        private readonly ILogger<AlgoApi> log;
        private readonly AlgodApi algod;
        private readonly IndexerApi indexer;

        public AlgoApi(ILogger<AlgoApi> log, IConfiguration config)
        {
            this.log = log;
            this.algod = new AlgodApi(config["Endpoints:Algod"], config["Node2ApiToken"]);
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

        public PendingTransactionResponse SubmitTransactionWait(SignedTransaction signedTxn)
        {
            PostTransactionsResponse id = Utils.SubmitTransaction(algod, signedTxn);
            PendingTransactionResponse resp = Utils.WaitTransactionToComplete(algod, id.TxId);
            return resp;
        }

        public PostTransactionsResponse SubmitTransaction(SignedTransaction signedTxn)
        {
            PostTransactionsResponse id = Utils.SubmitTransaction(algod, signedTxn);
            return id;
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

        public IEnumerable<string> GetAddressesSent(string senderAddress, long assetId, long minRound, long limit = 100)
        {
            TransactionsResponse transactionsResponse = this.GetAssetTransactions(senderAddress, "sender", assetId, minRound: minRound, limit: limit);

            List<string> walletAddresses = new List<string>();

            foreach (Transaction txn in transactionsResponse.Transactions)
            {
                walletAddresses.Add(txn.AssetTransferTransaction.Receiver);
            }

            while (transactionsResponse.NextToken != null)
            {
                transactionsResponse = this.GetAssetTransactions(senderAddress, "sender", assetId, minRound: minRound, limit: limit, next: transactionsResponse.NextToken);

                foreach (Transaction txn in transactionsResponse.Transactions)
                {
                    walletAddresses.Add(txn.AssetTransferTransaction.Receiver);
                }
            }

            return walletAddresses;
        }

        public TransactionsResponse GetAssetTransactions(string address, string addressRole, long assetId, long? currencyGreaterThan = null, 
            long? currencyLessThan = null, long? minRound = null, long limit = 100, string next = null)
        {
            TransactionsResponse transactionsResponse = null;
            int maxRetries = 5;
            int currentRetry = 0;

            while (transactionsResponse == null && currentRetry < maxRetries)
            {
                try
                {
                    if (next == null)
                    {
                        transactionsResponse = indexer.LookupAssetTransactions(
                            assetId,
                            address: address,
                            addressRole: addressRole,
                            minRound: minRound,
                            currencyGreaterThan: currencyGreaterThan,
                            currencyLessThan: currencyLessThan,
                            limit: limit);
                    }
                    else
                    {
                        transactionsResponse = indexer.LookupAssetTransactions(
                            assetId,
                            address: address,
                            addressRole: addressRole,
                            minRound: minRound,
                            currencyGreaterThan: currencyGreaterThan,
                            currencyLessThan: currencyLessThan,
                            limit: limit,
                            next: next);
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
                throw new Exception("GetAssetTransactions ran out of retries");
            }

            return transactionsResponse;
        }

        public IDictionary<string, long> GetAlgoReceived(string receiverAddress)
        {
            Dictionary<string, long> received = new Dictionary<string, long>();
            TransactionsResponse transactionsResponse = this.GetAssetTransactions(receiverAddress, "receiver", 0);

            foreach (Transaction txn in transactionsResponse.Transactions)
            {
                if (txn.PaymentTransaction != null &&
                    txn.PaymentTransaction.Amount.HasValue &&
                    txn.PaymentTransaction.Amount > 0)
                {
                    if (received.ContainsKey(txn.Sender))
                    {
                        received[txn.Sender] += txn.PaymentTransaction.Amount.Value;
                    }
                    else
                    {
                        received[txn.Sender] = txn.PaymentTransaction.Amount.Value;
                    }
                }
            }

            while (transactionsResponse.NextToken != null)
            {
                transactionsResponse = this.GetAssetTransactions(receiverAddress, "receiver", 0, next: transactionsResponse.NextToken);

                foreach (Transaction txn in transactionsResponse.Transactions)
                {
                    if (txn.PaymentTransaction != null &&
                        txn.PaymentTransaction.Amount.HasValue &&
                        txn.PaymentTransaction.Amount > 0)
                    {
                        if (!received.ContainsKey(txn.Sender))
                        {
                            received[txn.Sender] = txn.PaymentTransaction.Amount.Value;
                        }
                        else
                        {
                            received[txn.Sender] += txn.PaymentTransaction.Amount.Value;
                        }
                    }
                }
            }

            return received;
        }

        public long GetAssetLowest(string address, long assetId, long assetAmount, DateTime afterTime, long limit = 100)
        {
            TransactionsResponse transactionsResponse = this.GetAssetTransactions(address, assetId, afterTime, limit);

            long lowestAmount = assetAmount;

            foreach (Transaction txn in transactionsResponse.Transactions)
            {
                if (txn.Sender == address)
                {
                    assetAmount += (long)txn.AssetTransferTransaction.Amount;
                }
                else
                {
                    assetAmount -= (long)txn.AssetTransferTransaction.Amount;

                    if (assetAmount < lowestAmount)
                    {
                        lowestAmount = assetAmount;
                    }
                }
            }

            while (transactionsResponse.NextToken != null)
            {
                transactionsResponse = this.GetAssetTransactions(address, assetId, afterTime, limit, next: transactionsResponse.NextToken);

                foreach (Transaction txn in transactionsResponse.Transactions)
                {
                    if (txn.Sender == address)
                    {
                        assetAmount -= (long)txn.AssetTransferTransaction.Amount;

                        if (assetAmount < lowestAmount)
                        {
                            lowestAmount = assetAmount;
                        }
                    }
                    else
                    {
                        assetAmount += (long)txn.AssetTransferTransaction.Amount;
                    }
                }
            }

            return lowestAmount;
        }

        public TransactionsResponse GetAssetTransactions(string address, long assetId, DateTime afterTime, long limit = 100, string next = null)
        {
            TransactionsResponse transactionsResponse = null;
            int maxRetries = 5;
            int currentRetry = 0;

            while (transactionsResponse == null && currentRetry < maxRetries)
            {
                try
                {
                    if (next == null)
                    {
                        transactionsResponse = indexer.LookupAssetTransactions(
                            assetId,
                            address: address,
                            afterTime: afterTime,
                            limit: limit);
                    }
                    else
                    {
                        transactionsResponse = indexer.LookupAssetTransactions(
                            assetId,
                            address: address,
                            afterTime: afterTime,
                            limit: limit,
                            next: next);
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
                throw new Exception("GetAssetTransactions ran out of retries");
            }

            return transactionsResponse;
        }

        public IEnumerable<AssetValue> GetAccountAssetValues(string walletAddress, string unitNameContainsString = "", string projectId = null, long? value = null)
        {
            IEnumerable<AssetHolding> assetHoldings = this.GetAssetsByAddress(walletAddress);
            List<AssetValue> assetValues = new List<AssetValue>();

            foreach (AssetHolding assetHolding in assetHoldings)
            {
                Asset asset = this.GetAssetById(assetHolding.AssetId.Value);

                if (asset.Params.UnitName.Contains(unitNameContainsString))
                {
                    AssetValue assetValue = new AssetValue
                    {
                        Id = asset.Index.Value.ToString(),
                        AssetId = asset.Index.Value,
                        UnitName = asset.Params.UnitName,
                        Name = asset.Params.Name,
                    };

                    if (projectId != null)
                    {
                        assetValue.ProjectId = projectId;
                    }

                    if (value.HasValue)
                    {
                        assetValue.Value = value.Value;
                    }

                    assetValues.Add(assetValue);
                }
            }

            return assetValues;
        }

        public IEnumerable<AssetValue> GetAccountAssetValues(string walletAddress, Func<ulong, long> valueCalc, string unitNameContainsString = "", string projectId = null)
        {
            IEnumerable<AssetHolding> assetHoldings = this.GetAssetsByAddress(walletAddress);
            List<AssetValue> assetValues = new List<AssetValue>();

            foreach (AssetHolding assetHolding in assetHoldings)
            {
                Asset asset = this.GetAssetById(assetHolding.AssetId.Value);

                if (asset.Params.UnitName != null && asset.Params.UnitName.Contains(unitNameContainsString))
                {
                    AssetValue assetValue = new AssetValue
                    {
                        Id = asset.Index.Value.ToString(),
                        AssetId = asset.Index.Value,
                        UnitName = asset.Params.UnitName,
                        Name = asset.Params.Name,
                    };

                    if (projectId != null)
                    {
                        assetValue.ProjectId = projectId;
                    }

                    assetValue.Value = valueCalc(asset.Params.Total.Value);

                    assetValues.Add(assetValue);
                }
            }

            return assetValues;
        }

        public NodeStatusResponse GetStatusAfterRound(long round)
        {
            return algod.WaitForBlock(round);
        }

        public NodeStatusResponse GetStatus()
        {
            return algod.GetStatus();
        }

        public long? GetLastRound()
        {
            return this.GetStatus().LastRound;
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
