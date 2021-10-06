using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using System.Linq;
using Microsoft.Extensions.Logging;
using Algorand.V2;
using Algorand.V2.Model;
using System.Threading;

namespace Airdrop
{
    class ApiUtil
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

        public List<string> GetWalletAddressesWithAsset(int assetId)
        {
            var assetInfo = _indexer.LookupAssetBalances(assetId);

            List<string> walletAddresses = new List<string>();

            foreach (MiniAssetHolding account in assetInfo.Balances)
            {
                walletAddresses.Add(account.Address);
            }

            while (assetInfo.NextToken != null)
            {
                Thread.Sleep(1200); //PureStake sleep
                assetInfo = _indexer.LookupAssetBalances(assetId, next: assetInfo.NextToken);

                foreach (MiniAssetHolding account in assetInfo.Balances)
                {
                    walletAddresses.Add(account.Address);
                }
            }

            return walletAddresses;
        }

        public IEnumerable<string> GetWalletAddressesWithAsset(int assetId, params int[] assetIds)
        {
            IEnumerable<string> addresses = GetWalletAddressesWithAsset(assetId);

            foreach (int id in assetIds)
            {
                IEnumerable<string> intersectAddresses = GetWalletAddressesWithAsset(id);
                addresses = addresses.Intersect(intersectAddresses);
            }

            return addresses;
        }

        public long GetAssetDecimals(int assetId)
        {
            return _algod.GetAssetByID(assetId).Params.Decimals.Value;
        }

        public Account GetAccountByAddress(string walletAddress)
        {
            return _algod.AccountInformation(walletAddress);
        }
    }
}
