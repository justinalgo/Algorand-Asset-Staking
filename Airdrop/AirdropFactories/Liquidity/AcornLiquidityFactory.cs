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
            List<string> walletAddresses = (await this.FetchWalletAddresses()).ToList();
            walletAddresses.Remove(this.LiquidityWallet);
            walletAddresses.Remove(this.CreatorWallet);

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

        public async override Task<IEnumerable<string>> FetchWalletAddresses()
        {
            IEnumerable<string> walletAddresses = await this.indexerUtils.GetWalletAddresses(this.LiquidityAssetId, this.AssetId);

            return walletAddresses;
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
                    IEnumerable<Transaction> transactions = this.indexerUtils.GetTransactions(walletAddress, this.LiquidityAssetId, afterTime: DateTime.Now.AddDays(-2), txType: TxType.Axfer).Result;
                    ulong lowAmount = this.GetAssetLowest(walletAddress, liquidityAmount, transactions);
                    liquidityAmounts.Add((walletAddress, lowAmount));
                }
            });

            return Task.FromResult<IEnumerable<(string, ulong)>>(liquidityAmounts);
        }
    }
}
