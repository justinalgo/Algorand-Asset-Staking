using Algorand.V2.Model;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Util;

namespace Airdrop.AirdropFactories.Liquidity
{
    public class AlchemonLiquidityFactory : ILiquidityAirdropFactory
    {
        public long AssetId { get; set; }
        public long Decimals { get; set; }
        public long LiquidityAssetId { get; set; }
        public string LiquidityWallet { get; set; }
        public long DropTotal { get; set; }
        public long DropMinimum { get; set; }
        private readonly IAlgoApi api;

        public AlchemonLiquidityFactory(IAlgoApi api)
        {
            this.AssetId = 310014962;
            this.Decimals = 0;
            this.LiquidityAssetId = 359448756;
            this.LiquidityWallet = "O6CDEA7NCJASQSYSAMLPGLICTTPP3BGVOCPYFGCFT54O4SCXGN3ULAOYPU";
            this.DropTotal = 12500;
            this.DropMinimum = 0;
            this.api = api;
        }

        public Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts()
        {
            List<string> walletAddresses = this.FetchWalletAddresses().ToList();
            walletAddresses.Remove(this.LiquidityWallet);

            IEnumerable<(string, long)> liquidityAmounts = this.GetLiquidityAmounts(walletAddresses);

            long liquidityTotal = liquidityAmounts.Sum(la => la.Item2);

            List<AirdropAmount> airdropAmounts = new List<AirdropAmount>();

            foreach ((string, long) liquidityAmount in liquidityAmounts)
            {
                long dropAmount = (long)(this.DropTotal * ((double)liquidityAmount.Item2 / (double)liquidityTotal));
                if (dropAmount > DropMinimum)
                {
                    airdropAmounts.Add(new AirdropAmount(liquidityAmount.Item1, this.AssetId, dropAmount));
                }
            }

            return Task.FromResult<IEnumerable<AirdropAmount>>(airdropAmounts);
        }

        public IEnumerable<(string, long)> GetLiquidityAmounts(IEnumerable<string> walletAddresses)
        {
            ConcurrentBag<(string, long)> liquidityAmounts = new ConcurrentBag<(string, long)>();

            Parallel.ForEach(walletAddresses, new ParallelOptions { MaxDegreeOfParallelism = 20 }, walletAddress =>
            {
                Account account = api.GetAccountByAddress(walletAddress);
                ulong? liquidityAmount = this.GetLiquidityAssetAmount(account.Assets);

                if (liquidityAmount.HasValue)
                {
                    liquidityAmounts.Add((walletAddress, (long)liquidityAmount.Value));
                }
            });

            return liquidityAmounts;
        }

        public ulong? GetLiquidityAssetAmount(IEnumerable<AssetHolding> assetHoldings)
        {
            foreach (AssetHolding assetHolding in assetHoldings)
            {
                if (assetHolding.AssetId.HasValue &&
                    assetHolding.AssetId.Value == this.LiquidityAssetId &&
                    assetHolding.Amount.HasValue &&
                    assetHolding.Amount > 0)
                {
                    return assetHolding.Amount;
                }
            }

            return null;
        }

        public IEnumerable<string> FetchWalletAddresses()
        {
            IEnumerable<string> walletAddresses = this.api.GetWalletAddressesWithAsset(this.LiquidityAssetId, this.AssetId);

            return walletAddresses;
        }
    }
}
