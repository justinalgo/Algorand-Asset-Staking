using Algorand.V2.Algod.Model;
using Transaction = Algorand.V2.Indexer.Model.Transaction;
using TxType = Algorand.V2.Indexer.Model.TxType;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;
using System;

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
        public string[] RevokedAddresses { get; set; }
        public DateTime AfterTime { get; set; }
        public IIndexerUtils IndexerUtils { get; }
        public IAlgodUtils AlgodUtils { get; }

        public LiquidityAirdropFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils)
        {
            this.IndexerUtils = indexerUtils;
            this.AlgodUtils = algodUtils;
        }

        public virtual async Task<IEnumerable<Account>> FetchAccounts()
        {
            IEnumerable<string> addresses = await this.IndexerUtils.GetWalletAddresses(this.DropAssetId);

            ConcurrentBag<Account> accounts = new ConcurrentBag<Account>();

            Parallel.ForEach<string>(addresses, new ParallelOptions { MaxDegreeOfParallelism = 10 }, address =>
            {
                var account = this.AlgodUtils.GetAccount(address).Result;
                accounts.Add(account);
            });

            if (this.RevokedAddresses != null)
            {
                return accounts.Where(a => !this.RevokedAddresses.Contains(a.Address));
            }

            return accounts;
        }

        public virtual IEnumerable<(Account, ulong)> GetLiquidityAmounts(IEnumerable<Account> accounts)
        {
            ConcurrentBag<(Account, ulong)> liquidityAmounts = new ConcurrentBag<(Account, ulong)>();

            Parallel.ForEach(accounts, new ParallelOptions { MaxDegreeOfParallelism = 10 }, account =>
            {
                ulong liquidityAmount = GetLiquidityAssetAmount(account.Assets);

                if (liquidityAmount > this.LiquidityMinimum)
                {
                    var transactions = this.IndexerUtils.GetTransactions(account.Address, this.LiquidityAssetId, afterTime: this.AfterTime, txType: TxType.Axfer).Result;
                    ulong lowAmount = GetAssetLowest(account.Address, liquidityAmount, transactions);
                    liquidityAmounts.Add((account, lowAmount));
                }
            });

            return liquidityAmounts;
        }

        public async Task<IEnumerable<AirdropUnitCollection>> FetchAirdropUnitCollections()
        {
            IEnumerable<Account> accounts = await FetchAccounts();
            IEnumerable<(Account, ulong)> liquidityInfoItems = GetLiquidityAmounts(accounts);
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

        public double CalculateLiquidityValue(ulong liquidityTotal)
        {
            return (double)this.DropTotal / (double)liquidityTotal;
        }
    }
}
