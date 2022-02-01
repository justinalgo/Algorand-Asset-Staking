using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Liquidity
{
    public class AcornLiquidityFactory : LiquidityAirdropFactory
    {
        public string CreatorWallet { get; set; }
        public ulong DropTotal { get; set; }
        public ulong DropMinimum { get; set; }
        private readonly IIndexerUtils indexerUtils;

        public AcornLiquidityFactory(IIndexerUtils indexerUtils)
        {
            this.AssetId = 226265212;
            this.Decimals = 0;
            this.CreatorWallet = "GOGC4QDPXDK3WEVO2BYBS3LGSMTWDMQHDOO5KRJ7HLT6HDMI5FX7MLUMRA";
            this.LiquidityAssetId = 552642388;
            this.LiquidityWallet = "H3DSZ4DXLJQ7OXG3NQUSGS2NXDNQG4BVZRWJWGGWDOVORHRTLE2IVJYGT4";
            this.LiquidityMinimum = 0;
            this.DropTotal = 1000000;
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

        public async override Task<IEnumerable<Account>> FetchAccounts()
        {
            IEnumerable<Account> accounts = await this.indexerUtils.GetAccounts(this.LiquidityAssetId, this.AssetId);

            return accounts.Where(a => a.Address != this.CreatorWallet && a.Address != this.LiquidityWallet);
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
