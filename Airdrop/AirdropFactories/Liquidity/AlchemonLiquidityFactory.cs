using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Liquidity
{
    public class AlchemonLiquidityFactory : ILiquidityAirdropFactory
    {
        public ulong AssetId { get; set; }
        public ulong Decimals { get; set; }
        public ulong LiquidityAssetId { get; set; }
        public string LiquidityWallet { get; set; }
        public ulong LiquidityMinimum { get; set; }
        public ulong DropTotal { get; set; }
        public ulong DropMinimum { get; set; }
        private readonly IIndexerUtils indexerUtils;

        public AlchemonLiquidityFactory(IIndexerUtils indexerUtils)
        {
            this.AssetId = 310014962;
            this.Decimals = 0;
            this.LiquidityAssetId = 552701368;
            this.LiquidityWallet = "EJGN54S3OSQXDX5NYOGYZBGLIZZEKQSROO3AXKX2WPJ2CRMAW57YMDXWWE";
            this.LiquidityMinimum = 0;
            this.DropTotal = 12500;
            this.DropMinimum = 0;
            this.indexerUtils = indexerUtils;
        }

        public async Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts()
        {
            List<string> walletAddresses = (await this.FetchWalletAddresses()).ToList();
            walletAddresses.Remove(this.LiquidityWallet);

            IEnumerable<(string, ulong)> liquidityAmounts = await this.GetLiquidityAmounts(walletAddresses);

            ulong liquidityTotal = (ulong)liquidityAmounts.Sum(la => (double)la.Item2);
            
            List<AirdropAmount> airdropAmounts = new List<AirdropAmount>();

            foreach ((string, ulong) liquidityAmount in liquidityAmounts)
            {
                ulong dropAmount = (ulong)(this.DropTotal * ((double)liquidityAmount.Item2 / (double)liquidityTotal));
                if (dropAmount > DropMinimum)
                {
                    airdropAmounts.Add(new AirdropAmount(liquidityAmount.Item1, this.AssetId, dropAmount));
                }
            }

            return airdropAmounts;
        }

        public async Task<IEnumerable<(string, ulong)>> GetLiquidityAmounts(IEnumerable<string> walletAddresses)
        {
            ConcurrentBag<(string, ulong)> liquidityAmounts = new ConcurrentBag<(string, ulong)>();

            //Parallel.ForEach(walletAddresses, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async walletAddress =>
            foreach (string walletAddress in walletAddresses)
            {
                Account account = await this.indexerUtils.GetAccount(walletAddress);
                ulong liquidityAmount = this.GetLiquidityAssetAmount(account.Assets);
                Console.WriteLine(walletAddress + " : " + liquidityAmount);

                if (liquidityAmount > this.LiquidityMinimum)
                {
                    IEnumerable<Transaction> transactions = await this.indexerUtils.GetTransactions(walletAddress, this.LiquidityAssetId, afterTime: DateTime.Now.AddDays(-2), txType: TxType.Axfer);
                    ulong lowAmount = this.GetAssetLowest(walletAddress, liquidityAmount, transactions);
                    liquidityAmounts.Add((walletAddress, lowAmount));
                }
            };

            //return Task.FromResult<IEnumerable<(string, ulong)>>(liquidityAmounts);
            return liquidityAmounts;
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

        public async Task<IEnumerable<string>> FetchWalletAddresses()
        {
            IEnumerable<string> walletAddresses = await this.indexerUtils.GetWalletAddresses(this.LiquidityAssetId, this.AssetId);

            return walletAddresses;
        }

        public ulong GetAssetLowest(string address, ulong amount, IEnumerable<Transaction> transactions)
        {
            ulong lowest = amount;

            foreach(Transaction transaction in transactions)
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
    }
}
