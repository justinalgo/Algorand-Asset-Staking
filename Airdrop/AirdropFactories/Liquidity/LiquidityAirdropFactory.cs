using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Liquidity
{
    public abstract class LiquidityAirdropFactory
    {
        public ulong DropAssetId { get; set; }
        public ulong Decimals { get; set; }
        public ulong LiquidityAssetId { get; set; }
        public string LiquidityWallet { get; set; }
        public ulong LiquidityMinimum { get; set; }
        public ulong DropTotal { get; set; }
        public ulong DropMinimum { get; set; }
        public abstract Task<IEnumerable<Account>> FetchAccounts();

        public abstract IEnumerable<(Account, ulong)> GetLiquidityAmounts(IEnumerable<Account> account);

        public async Task<IEnumerable<AirdropUnitCollection>> FetchAirdropUnitCollections()
        {
            IEnumerable<Account> accounts = await this.FetchAccounts();
            IEnumerable<(Account, ulong)> liquidityInfoItems = this.GetLiquidityAmounts(accounts);
            AirdropUnitCollectionManager collectionManager = new AirdropUnitCollectionManager();

            ulong liquidityTotal = (ulong)liquidityInfoItems.Sum(la => (double)la.Item2);
            double dropValue = this.CalculateLiquidityValue(liquidityTotal);

            foreach ((Account, ulong) liquidityInfo in liquidityInfoItems)
            {
                Account account = liquidityInfo.Item1;
                ulong liquidityAmount = liquidityInfo.Item2;
                ulong dropAmount = (ulong)(liquidityAmount * dropValue);

                if (dropAmount > DropMinimum)
                {
                    collectionManager.AddAirdropUnit(new AirdropUnit(
                        account.Address,
                        this.DropAssetId,
                        this.LiquidityAssetId,
                        dropValue,
                        numberOfSourceAsset: liquidityAmount,
                        isMultiplied: true));
                }
            }

            return collectionManager.GetAirdropUnitCollections();
        }

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

        public double CalculateLiquidityValue( ulong liquidityTotal)
        {
            return (double)this.DropTotal / (double)liquidityTotal;
        }
    }
}
