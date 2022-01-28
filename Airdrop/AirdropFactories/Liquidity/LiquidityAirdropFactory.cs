using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Liquidity
{
    public abstract class LiquidityAirdropFactory : ILiquidityAirdropFactory
    {
        public ulong LiquidityAssetId { get; set; }
        public string LiquidityWallet { get; set; }
        public ulong LiquidityMinimum { get; set; }
        public ulong AssetId { get; set; }
        public ulong Decimals { get; set; }
        public abstract Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts();

        public abstract Task<IEnumerable<string>> FetchWalletAddresses();

        public abstract Task<IEnumerable<(string, ulong)>> GetLiquidityAmounts(IEnumerable<string> walletAddresses);

        public ulong GetLiquidityAssetAmount(IEnumerable<AssetHolding> assetHoldings)
        {
            foreach (AssetHolding assetHolding in assetHoldings)
            {
                if (assetHolding.AssetId == this.LiquidityAssetId)
                {
                    return assetHolding.Amount;
                }
            }

            return 0;
        }

        public ulong GetAssetLowest(string address, ulong amount, IEnumerable<Transaction> transactions)
        {
            ulong lowest = amount;

            foreach (Transaction transaction in transactions)
            {
                if (transaction.Sender == address)
                {
                    amount += transaction.AssetTransferTransaction.Amount;
                }
                else
                {
                    amount -= transaction.AssetTransferTransaction.Amount;
                }

                if (amount < lowest)
                {
                    lowest = amount;
                }
            }

            return lowest;
        }

        public ulong CalculateDropAmount(ulong dropTotal, ulong liquidityTotal, ulong liquidityAmount)
        {
            return (ulong)(dropTotal * ((double)liquidityAmount / (double)liquidityTotal));
        }
    }
}
