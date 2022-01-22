using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Util;
using Util.Cosmos;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Holdings
{
    public class CryptoBunnyHoldingsFactory : IHoldingsAirdropFactory
    {
        public ulong AssetId { get; set; }
        public ulong Decimals { get; set; }
        public string[] CreatorAddresses { get; set; }
        private readonly IIndexerUtils indexerUtils;
        private readonly ICosmos cosmos;

        public CryptoBunnyHoldingsFactory(IIndexerUtils indexerUtils, ICosmos cosmos)
        {
            this.AssetId = 329532956;
            this.Decimals = 0;
            this.CreatorAddresses = new string[] { "BNYSETPFTL2657B5RCSW64A3M766GYBVRV5ALOM7F7LIRUZKBEOGF6YSO4" };
            this.indexerUtils = indexerUtils;
            this.cosmos = cosmos;
        }

        public async Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts()
        {
            IDictionary<ulong, ulong> assetValues = await this.FetchAssetValues();
            List<AirdropAmount> airdropAmounts = new List<AirdropAmount>();
            IEnumerable<string> walletAddresses = await this.FetchWalletAddresses();

            foreach (string walletAddress in walletAddresses)
            {
                Account account = await this.indexerUtils.GetAccount(walletAddress);
                IEnumerable<AssetHolding> assetHoldings = account.Assets;
                ulong amount = this.GetAssetHoldingsAmount(assetHoldings, assetValues);
                if (amount > 0)
                {
                    airdropAmounts.Add(new AirdropAmount(walletAddress, this.AssetId, amount));
                }
            }

            return airdropAmounts;
        }

        public async Task<IEnumerable<string>> FetchWalletAddresses()
        {
            IEnumerable<string> walletAddresses = await this.indexerUtils.GetWalletAddresses(this.AssetId);

            return walletAddresses;
        }

        public async Task<IDictionary<ulong, ulong>> FetchAssetValues()
        {
            IEnumerable<AssetValue> assets = await cosmos.GetAssetValues("CryptoBunny");

            Dictionary<ulong, ulong> assetValues = assets.ToDictionary(av => av.AssetId, av => av.Value);

            return assetValues;
        }

        public ulong GetAssetHoldingsAmount(IEnumerable<AssetHolding> assetHoldings, IDictionary<ulong, ulong> assetValues)
        {
            ulong airdropAmount = 0;

            foreach (AssetHolding miniAssetHolding in assetHoldings)
            {
                if (miniAssetHolding.Amount > 0 &&
                    assetValues.ContainsKey(miniAssetHolding.AssetId))
                {
                    airdropAmount += (ulong)(assetValues[miniAssetHolding.AssetId] * Math.Pow(10, this.Decimals) * miniAssetHolding.Amount);
                }
            }

            return airdropAmount;
        }
    }
}
