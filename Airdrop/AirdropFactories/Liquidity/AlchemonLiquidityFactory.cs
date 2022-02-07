﻿using Algorand.V2.Indexer.Model;
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
        private readonly IIndexerUtils indexerUtils;

        public AlchemonLiquidityFactory(IIndexerUtils indexerUtils)
        {
            this.DropAssetId = 310014962;
            this.Decimals = 0;
            this.LiquidityAssetId = 552701368;
            this.LiquidityWallet = "EJGN54S3OSQXDX5NYOGYZBGLIZZEKQSROO3AXKX2WPJ2CRMAW57YMDXWWE";
            this.LiquidityMinimum = 0;
            this.DropTotal = 12500;
            this.DropMinimum = 0;
            this.indexerUtils = indexerUtils;
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
            IEnumerable<Account> accounts = await this.indexerUtils.GetAccounts(new[] { this.LiquidityAssetId, this.DropAssetId });

            return accounts.Where(a => a.Address != this.LiquidityWallet);
        }
    }
}
