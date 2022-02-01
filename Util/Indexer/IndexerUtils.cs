﻿using Algorand.V2;
using Algorand.V2.Indexer;
using Algorand.V2.Indexer.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Indexer
{
    public class IndexerUtils : IIndexerUtils
    {
        private readonly ILogger<IndexerUtils> log;
        private readonly ILookupApi lookupApi;
        private readonly ISearchApi searchApi;

        public IndexerUtils(ILogger<IndexerUtils> log, ILookupApi lookupApi, ISearchApi searchApi)
        {
            this.log = log;
            this.lookupApi = lookupApi;
            this.searchApi = searchApi;
        }

        public async Task<IEnumerable<Account>> GetAccounts(ulong assetId)
        {
            List<Account> accounts = new List<Account>();

            Response response = await this.searchApi.AccountsAsync(asset_id: assetId, include_all: false);

            accounts.AddRange(response.Accounts);

            while (response.NextToken != null)
            {
                response = await this.searchApi.AccountsAsync(asset_id: assetId, include_all: false, next: response.NextToken);

                accounts.AddRange(response.Accounts);
            }

            return accounts;
        }

        public async Task<IEnumerable<Account>> GetAccounts(ulong assetId, params ulong[] assetIds)
        {
            IEnumerable<Account> accounts = await this.GetAccounts(assetId);
            List<Account> cleanedAccounts = new List<Account>();

            foreach(Account account in accounts)
            {
                HashSet<ulong> accountAssets = account.Assets.Select(a => a.AssetId).ToHashSet();

                bool containsAllAssets = true;

                foreach (ulong id in assetIds)
                {
                    if (!accountAssets.Contains(id))
                    {
                        containsAllAssets = false;
                        break;
                    }
                }

                if (containsAllAssets)
                {
                    cleanedAccounts.Add(account);
                }
            }

            return cleanedAccounts;
        }

        public async Task<IEnumerable<string>> GetWalletAddresses(ulong assetId)
        {
            IEnumerable<Account> accounts = await this.GetAccounts(assetId);

            return accounts.Select(a => a.Address);
        }

        public async Task<IEnumerable<string>> GetWalletAddresses(ulong assetId, params ulong[] assetIds)
        {
            IEnumerable<Account> accounts = await this.GetAccounts(assetId, assetIds);

            return accounts.Select(a => a.Address);
        }

        public async Task<IEnumerable<string>> GetWalletAddresses(string address, ulong assetId, AddressRole? addressRole = null, TxType? txType = null, ulong? currencyGreaterThan = null, ulong? currencyLessThan = null, ulong? minRound = null, DateTimeOffset? afterTime = null)
        {
            List<string> walletAddresses = new List<string>();

            IEnumerable<Transaction> transactions = await GetTransactions(address, assetId, addressRole, txType, currencyGreaterThan, currencyLessThan, minRound);

            foreach (Transaction transaction in transactions)
            {
                if (addressRole == AddressRole.Receiver)
                {
                    walletAddresses.Add(transaction.Sender);
                }
                else
                {
                    switch (transaction.TxType)
                    {
                        case TransactionTxType.Axfer:
                            walletAddresses.Add(transaction.AssetTransferTransaction.Receiver);
                            break;
                        case TransactionTxType.Pay:
                            walletAddresses.Add(transaction.PaymentTransaction.Receiver);
                            break;
                        case TransactionTxType.Afrz:
                            walletAddresses.Add(transaction.AssetFreezeTransaction.Address);
                            break;
                        default:
                            break;
                    }
                }
            }

            return walletAddresses;
        }
        public async Task<Asset> GetAsset(ulong assetId)
        {
            Response9 response = await lookupApi.AssetsAsync(assetId);

            return response.Asset;
        }
        public async Task<IEnumerable<Asset>> GetAsset(IEnumerable<ulong> assetIds)
        {
            List<Asset> assets = new List<Asset>();

            foreach (ulong assetId in assetIds)
            {
                assets.Add(await GetAsset(assetId));
            }

            return assets;
        }
        public async Task<IEnumerable<Transaction>> GetTransactions(string address, ulong assetId, AddressRole? addressRole = null, TxType? txType = null, ulong? currencyGreaterThan = null, ulong? currencyLessThan = null, ulong? minRound = null, DateTimeOffset? afterTime = null)
        {
            List<Transaction> transactions = new List<Transaction>();

            Response4 response = await searchApi.TransactionsAsync(
                    tx_type: txType,
                    min_round: minRound,
                    asset_id: assetId,
                    currency_greater_than: currencyGreaterThan,
                    currency_less_than: currencyLessThan,
                    address: address,
                    address_role: addressRole,
                    after_time: afterTime
                );

            transactions.AddRange(response.Transactions);

            while (response.NextToken != null)
            {
                response = await searchApi.TransactionsAsync(
                    next: response.NextToken,
                    min_round: minRound,
                    asset_id: assetId,
                    after_time: afterTime,
                    currency_greater_than: currencyGreaterThan,
                    currency_less_than: currencyLessThan,
                    address: address,
                    address_role: addressRole
                );

                transactions.AddRange(response.Transactions);
            }

            return transactions;
        }
        public async Task<Account> GetAccount(string address)
        {
            Response6 response = await lookupApi.AccountsAsync(address);

            return response.Account;
        }
        public async Task<IEnumerable<MiniAssetHolding>> GetBalances(ulong assetId)
        {
            List<MiniAssetHolding> miniAssetHoldings = new List<MiniAssetHolding>();

            Response10 response = await this.lookupApi.BalancesAsync(assetId);

            miniAssetHoldings.AddRange(response.Balances);

            while (response.NextToken != null)
            {
                miniAssetHoldings.AddRange(response.Balances);
            }

            return response.Balances;
        }
    }
}
