using Algorand.V2.Model;
using System.Collections.Generic;

namespace Airdrop.AirdropFactories.Liquidity
{
    public interface ILiquidityAirdropFactory : IAirdropFactory
    {
        public long LiquidityAssetId { get; set; }
        public string LiquidityWallet { get; set; }
        public ulong? GetLiquidityAssetAmount(IEnumerable<AssetHolding> assetHoldings);
        public IEnumerable<(string, long)> GetLiquidityAmounts(IEnumerable<string> walletAddresses);
    }
}
