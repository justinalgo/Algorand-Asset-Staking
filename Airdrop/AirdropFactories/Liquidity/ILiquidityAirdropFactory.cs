using Algorand.V2.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Airdrop.AirdropFactories.Liquidity
{
    interface ILiquidityAirdropFactory : IAirdropFactory
    {
        public long LiquidityAssetId { get; set; }
        public string LiquidityWallet { get; set; }
        public ulong? GetLiquidityAssetAmount(IEnumerable<AssetHolding> assetHoldings);
        public IEnumerable<(string, long)> GetLiquidityAmounts(IEnumerable<string> walletAddresses);
    }
}
