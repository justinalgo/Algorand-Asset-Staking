using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Airdrop.AirdropFactories.Holdings
{
    public abstract class HoldingsAirdropFactory : IHoldingsAirdropFactory
    {
        public string[] CreatorAddresses { get; set; }
        public ulong AssetId { get; set; }
        public ulong Decimals { get; set; }
        public abstract Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts();

        public abstract Task<IDictionary<ulong, ulong>> FetchAssetValues();

        public abstract Task<IEnumerable<Account>> FetchAccounts();

        public ulong GetAssetHoldingsAmount(IEnumerable<AssetHolding> assetHoldings, IDictionary<ulong, ulong> assetValues)
        {
            ulong airdropAmount = 0;

            if (assetHoldings != null)
            {
                foreach (AssetHolding miniAssetHolding in assetHoldings)
                {
                    if (miniAssetHolding.Amount > 0 &&
                        assetValues.ContainsKey(miniAssetHolding.AssetId))
                    {
                        airdropAmount += (ulong)(assetValues[miniAssetHolding.AssetId] * Math.Pow(10, this.Decimals) * miniAssetHolding.Amount);
                    }
                }
            }

            return airdropAmount;
        }
    }
}
