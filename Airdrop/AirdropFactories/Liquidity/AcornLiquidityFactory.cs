using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Liquidity
{
    public class AcornLiquidityFactory : LiquidityAirdropFactory
    {
        private readonly IIndexerUtils indexerUtils;
        public string CreatorWallet { get; set; }

        public AcornLiquidityFactory(IIndexerUtils indexerUtils)
        {
            this.DropAssetId = 226265212;
            this.Decimals = 0;
            this.CreatorWallet = "GOGC4QDPXDK3WEVO2BYBS3LGSMTWDMQHDOO5KRJ7HLT6HDMI5FX7MLUMRA";
            this.LiquidityAssetId = 552642388;
            this.LiquidityWallet = "H3DSZ4DXLJQ7OXG3NQUSGS2NXDNQG4BVZRWJWGGWDOVORHRTLE2IVJYGT4";
            this.LiquidityMinimum = 0;
            this.DropTotal = 1000000;
            this.DropMinimum = 0;
            this.indexerUtils = indexerUtils;
        }

        public async override Task<IEnumerable<Account>> FetchAccounts()
        {
            IEnumerable<Account> accounts = await this.indexerUtils.GetAccounts(new[] { this.LiquidityAssetId, this.DropAssetId });

            accounts = accounts.Where(a => a.Address != this.CreatorWallet && a.Address != this.LiquidityWallet);

            return accounts;
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
    }
}
