using Algorand.V2;
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
        private readonly LookupApi lookupApi;
        private readonly SearchApi searchApi;
        private readonly CommonApi commonApi;

        public IndexerUtils(ILogger<IndexerUtils> log, IConfiguration config)
        {
            this.log = log;
            var httpClient = HttpClientConfigurator.ConfigureHttpClient(config["Endpoints:Indexer"], config["IndexerToken"]);
            lookupApi = new LookupApi(httpClient) { BaseUrl = config["Endpoints:Indexer"] };
            searchApi = new SearchApi(httpClient) { BaseUrl = config["Endpoints:Indexer"] };
            commonApi = new CommonApi(httpClient) { BaseUrl = config["Endpoints:Indexer"] };
        }

        public async Task<IEnumerable<string>> GetWalletAddresses(ulong assetId)
        {
            List<string> walletAddresses = new List<string>();

            Response response = await this.searchApi.AccountsAsync(asset_id: assetId);

            foreach (Account account in response.Accounts)
            {
                walletAddresses.Add(account.Address);
            }

            while (response.NextToken != null)
            {
                response = await this.searchApi.AccountsAsync(asset_id: assetId, next: response.NextToken);

                foreach (Account account in response.Accounts)
                {
                    walletAddresses.Add(account.Address);
                }
            }

            return walletAddresses;
        }
        public async Task<IEnumerable<string>> GetWalletAddresses(ulong assetId, params ulong[] assetIds)
        {
            IEnumerable<string> addresses = await GetWalletAddresses(assetId);

            foreach (ulong id in assetIds)
            {
                IEnumerable<string> intersectAddresses = await GetWalletAddresses(id);
                addresses = addresses.Intersect(intersectAddresses);
            }

            return addresses;
        }
        public async Task<IEnumerable<string>> GetWalletAddresses(string address, ulong assetId, AddressRole? addressRole = null, TxType? txType = null, ulong? currencyGreaterThan = null, ulong? currencyLessThan = null, ulong? minRound = null, DateTimeOffset? afterTime = null)
        {
            List<string> walletAddresses = new List<string>();

            IEnumerable<Transaction> transactions = await GetTransactions(address, assetId, addressRole, txType, currencyGreaterThan, currencyLessThan, minRound);

            Func<Transaction, string> getAddress;

            if (addressRole == AddressRole.Sender)
            {
                getAddress = (Transaction txn) => txn.Sender;
            }
            else
            {
                switch (txType)
                {
                    case TxType.Axfer:
                        getAddress = (Transaction txn) => txn.AssetTransferTransaction.Receiver;
                        break;
                    case TxType.Pay:
                        getAddress = (Transaction txn) => txn.PaymentTransaction.Receiver;
                        break;
                    case TxType.Afrz:
                        getAddress = (Transaction txn) => txn.AssetFreezeTransaction.Address;
                        break;
                    default:
                        getAddress = (Transaction txn) => null;
                        break;
                }
            }

            foreach (Transaction transaction in transactions)
            {
                string addr = getAddress(transaction);
                if (addr != null)
                {
                    walletAddresses.Add(addr);
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
