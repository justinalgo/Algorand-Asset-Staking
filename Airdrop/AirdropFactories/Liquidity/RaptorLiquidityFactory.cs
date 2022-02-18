using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Liquidity
{
    public class RaptorLiquidityFactory : LiquidityAirdropFactory
    {
        public string CreatorWallet { get; set; }
        private readonly IIndexerUtils indexerUtils;

        public RaptorLiquidityFactory(IIndexerUtils indexerUtils)
        {
            this.DropAssetId = 426980914;
            this.Decimals = 2;
            this.CreatorWallet = "SBKN5JI72DS4USUUIFO3MMNZLPVDKERA2D3HOPZMSXAK5VBBEM364TGS3A";
            this.LiquidityAssetId = 428917669;
            this.LiquidityWallet = "AIFU57RAPPX672WDLAUGZE46677XNGYQEX2KQM54DJZ4HUTQ264M2TALQA";
            this.LiquidityMinimum = 400000;
            this.DropTotal = 20000000;
            this.DropMinimum = 0;
            this.indexerUtils = indexerUtils;
        }

        public override async Task<IEnumerable<Account>> FetchAccounts()
        {
            IEnumerable<Account> accounts = await this.indexerUtils.GetAccounts(new[] { this.LiquidityAssetId, this.DropAssetId });

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
                    IEnumerable<Transaction> transactions = this.indexerUtils.GetTransactions(account.Address, this.LiquidityAssetId, afterTime: DateTime.Now.AddDays(-7), txType: TxType.Axfer).Result;
                    ulong lowAmount = this.GetAssetLowest(account.Address, liquidityAmount, transactions);
                    liquidityAmounts.Add((account, lowAmount));
                }
            });

            return liquidityAmounts;
        }
    }
}
