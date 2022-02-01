using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Liquidity
{
    public class AlchemonLiquidityFactory : LiquidityAirdropFactory
    {
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

        public override async Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts()
        {
            IEnumerable<Account> accounts = await this.FetchAccounts();

            IEnumerable<(Account, ulong)> liquidityAmounts = this.GetLiquidityAmounts(accounts);

            ulong liquidityTotal = (ulong)liquidityAmounts.Sum(la => (double)la.Item2);
            
            List<AirdropAmount> airdropAmounts = new List<AirdropAmount>();

            foreach ((Account, ulong) liquidityAmount in liquidityAmounts)
            {
                ulong dropAmount = this.CalculateDropAmount(DropTotal, liquidityTotal, liquidityAmount.Item2);
                if (dropAmount > DropMinimum)
                {
                    airdropAmounts.Add(new AirdropAmount(liquidityAmount.Item1.Address, this.AssetId, dropAmount));
                }
            }

            return airdropAmounts;
        }

        public override IEnumerable<(Account, ulong)> GetLiquidityAmounts(IEnumerable<Account> accounts)
        {
            ConcurrentBag<(Account, ulong)> liquidityAmounts = new ConcurrentBag<(Account, ulong)>();

            Parallel.ForEach(accounts, new ParallelOptions { MaxDegreeOfParallelism = 10 }, account =>
            {
                ulong liquidityAmount = this.GetLiquidityAssetAmount(account.Assets);

                if (liquidityAmount > this.LiquidityMinimum)
                {
                    IEnumerable<Transaction> transactions = this.indexerUtils.GetTransactions(account.Address, this.LiquidityAssetId, afterTime: DateTime.Now.AddDays(-6.5), txType: TxType.Axfer).Result;
                    ulong lowAmount = this.GetAssetLowest(account.Address, liquidityAmount, transactions);
                    liquidityAmounts.Add((account, lowAmount));
                }
            });

            return liquidityAmounts;
        }

        public override async Task<IEnumerable<Account>> FetchAccounts()
        {
            IEnumerable<Account> accounts = await this.indexerUtils.GetAccounts(this.LiquidityAssetId, this.AssetId);

            return accounts.Where(a => a.Address != this.LiquidityWallet);
        }
    }
}
