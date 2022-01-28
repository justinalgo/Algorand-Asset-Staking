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
            List<string> walletAddresses = (await this.FetchWalletAddresses()).ToList();
            walletAddresses.Remove(this.LiquidityWallet);

            IEnumerable<(string, ulong)> liquidityAmounts = await this.GetLiquidityAmounts(walletAddresses);

            ulong liquidityTotal = (ulong)liquidityAmounts.Sum(la => (double)la.Item2);
            
            List<AirdropAmount> airdropAmounts = new List<AirdropAmount>();

            foreach ((string, ulong) liquidityAmount in liquidityAmounts)
            {
                ulong dropAmount = this.CalculateDropAmount(DropTotal, liquidityTotal, liquidityAmount.Item2);
                if (dropAmount > DropMinimum)
                {
                    airdropAmounts.Add(new AirdropAmount(liquidityAmount.Item1, this.AssetId, dropAmount));
                }
            }

            return airdropAmounts;
        }

        public override Task<IEnumerable<(string, ulong)>> GetLiquidityAmounts(IEnumerable<string> walletAddresses)
        {
            ConcurrentBag<(string, ulong)> liquidityAmounts = new ConcurrentBag<(string, ulong)>();

            Parallel.ForEach(walletAddresses, new ParallelOptions { MaxDegreeOfParallelism = 10 }, walletAddress =>
            {
                Account account = this.indexerUtils.GetAccount(walletAddress).Result;
                ulong liquidityAmount = this.GetLiquidityAssetAmount(account.Assets);

                if (liquidityAmount > this.LiquidityMinimum)
                {
                    Console.WriteLine(walletAddress);
                    IEnumerable<Transaction> transactions = this.indexerUtils.GetTransactions(walletAddress, this.LiquidityAssetId, afterTime: DateTime.Now.AddDays(-7), txType: TxType.Axfer).Result;
                    ulong lowAmount = this.GetAssetLowest(walletAddress, liquidityAmount, transactions);
                    liquidityAmounts.Add((walletAddress, lowAmount));
                }
            });

            return Task.FromResult<IEnumerable<(string, ulong)>>(liquidityAmounts);
        }

        public override async Task<IEnumerable<string>> FetchWalletAddresses()
        {
            IEnumerable<string> walletAddresses = await this.indexerUtils.GetWalletAddresses(this.LiquidityAssetId, this.AssetId);

            return walletAddresses;
        }
    }
}
