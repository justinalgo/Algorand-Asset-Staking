using Algorand.V2.Indexer.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Airdrop.AirdropFactories.Liquidity
{
    public interface ILiquidityAirdropFactory : IAirdropFactory
    {
        public ulong LiquidityAssetId { get; set; }
        public string LiquidityWallet { get; set; }
        ulong LiquidityMinimum { get; set; }
        public IEnumerable<(Account, ulong)> GetLiquidityAmounts(IEnumerable<Account> account);
        public ulong GetLiquidityAssetAmount(IEnumerable<AssetHolding> assetHoldings);
    }
}
