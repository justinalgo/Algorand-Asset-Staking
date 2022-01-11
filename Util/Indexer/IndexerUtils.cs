using Algorand.V2;
using Algorand.V2.Indexer;
using Algorand.V2.Indexer.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

    }
}
