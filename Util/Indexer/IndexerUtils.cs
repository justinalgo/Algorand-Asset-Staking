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
    public class IndexerUtils
    {
        private readonly ILogger<IndexerUtils> log;
        private readonly LookupApi lookupApi;
        private readonly SearchApi searchApi;
        private readonly CommonApi commonApi;
        private readonly int limit = 100;

        public IndexerUtils(ILogger<IndexerUtils> log, IConfiguration config)
        {
            this.log = log;
            var httpClient = HttpClientConfigurator.ConfigureHttpClient(config["Endpoints:Indexer"], config["IndexerToken"]);
            lookupApi = new LookupApi(httpClient) { BaseUrl = config["Endpoints:Indexer"] };
            searchApi = new SearchApi(httpClient) { BaseUrl = config["Endpoints:Indexer"] };
            commonApi = new CommonApi(httpClient) { BaseUrl = config["Endpoints:Indexer"] };
        }

        public async Task<IEnumerable<string>> GetWalletAddresses(int assetId)
        {
            List<string> walletAddresses = new List<string>();

            Response response = await this.searchApi.AccountsAsync(asset_id: assetId,
                                         limit: limit,
                                         next: null,
                                         currency_greater_than: null,
                                         include_all: false,
                                         currency_less_than: null,
                                         auth_addr: null,
                                         round: null,
                                         application_id: null);

            foreach (Account account in response.Accounts)
            {
                walletAddresses.Add(account.Address);
            }

            while (response.NextToken != null)
            {
                response = await this.searchApi.AccountsAsync(asset_id: assetId,
                                         limit: limit,
                                         next: response.NextToken,
                                         currency_greater_than: null,
                                         include_all: false,
                                         currency_less_than: null,
                                         auth_addr: null,
                                         round: null,
                                         application_id: null);

                foreach (Account account in response.Accounts)
                {
                    walletAddresses.Add(account.Address);
                }
            }

            return walletAddresses;
        }

        public async Task<IEnumerable<string>> GetWalletAddresses(int assetId, params int[] assetIds)
        {
            IEnumerable<string> addresses = await GetWalletAddresses(assetId);

            foreach (int id in assetIds)
            {
                IEnumerable<string> intersectAddresses = await GetWalletAddresses(id);
                addresses = addresses.Intersect(intersectAddresses);
            }

            return addresses;
        }

        public async Task<IEnumerable<string>> GetWalletAddresses(string address, AddressRole addressRole, int assetId, TxType txType, ulong? currencyGreaterThan = null, ulong? currencyLessThan = null, ulong? minRound = null)
        {
            List<string> walletAddresses = new List<string>();

            IEnumerable<Transaction> transactions = await GetTransactions(address, addressRole, assetId, txType, currencyGreaterThan, currencyLessThan, minRound);

            Func<Transaction, string> getAddress;

            if (addressRole == AddressRole.Sender)
            {
                getAddress = (Transaction txn) => txn.Sender;
            }
            else
            {
                switch(txType)
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
            Response9 response = await lookupApi.AssetsAsync(assetId, null);

            return response.Asset;
        }

        public async Task<IEnumerable<Asset>> GetAsset(IEnumerable<int> assetIds)
        {
            List<Asset> assets = new List<Asset>();

            foreach (int assetId in assetIds)
            {
                assets.Add(await GetAsset(assetId));
            }

            return assets;
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(string address, AddressRole addressRole, int assetId, TxType txType, ulong? currencyGreaterThan = null, ulong? currencyLessThan = null, ulong? minRound = null)
        {
            List<Transaction> transactions = new List<Transaction>();

            Response4 response = await searchApi.TransactionsAsync(
                    limit: 100,
                    next: null,
                    note_prefix: null,
                    tx_type: txType,
                    sig_type: null,
                    txid: null,
                    round: null,
                    min_round: minRound,
                    max_round: null,
                    asset_id: assetId,
                    before_time: null,
                    after_time: null,
                    currency_greater_than: currencyGreaterThan,
                    currency_less_than: currencyLessThan,
                    address: address,
                    address_role: addressRole,
                    exclude_close_to: null,
                    rekey_to: null,
                    application_id: null
                );

            transactions.AddRange(response.Transactions);

            while (response.NextToken != null)
            {
                response = await searchApi.TransactionsAsync(
                    limit: 100,
                    next: response.NextToken,
                    note_prefix: null,
                    tx_type: null,
                    sig_type: null,
                    txid: null,
                    round: null,
                    min_round: minRound,
                    max_round: null,
                    asset_id: assetId,
                    before_time: null,
                    after_time: null,
                    currency_greater_than: currencyGreaterThan,
                    currency_less_than: currencyLessThan,
                    address: address,
                    address_role: addressRole,
                    exclude_close_to: null,
                    rekey_to: null,
                    application_id: null
                );

                transactions.AddRange(response.Transactions);
            }

            return transactions;
        }
    }
}
