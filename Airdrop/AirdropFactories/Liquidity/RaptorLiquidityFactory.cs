using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Util;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Liquidity
{
    public class RaptorLiquidityFactory : ILiquidityAirdropFactory
    {
        public ulong AssetId { get; set; }
        public ulong Decimals { get; set; }
        public string CreatorWallet { get; set; }
        public ulong LiquidityAssetId { get; set; }
        public string LiquidityWallet { get; set; }
        public ulong LiquidityMinimum { get; set; }
        public ulong DropTotal { get; set; }
        public ulong DropMinimum { get; set; }

        private readonly IIndexerUtils indexerUtils;

        public RaptorLiquidityFactory(IIndexerUtils indexerUtils)
        {
            this.AssetId = 426980914;
            this.Decimals = 2;
            this.CreatorWallet = "SBKN5JI72DS4USUUIFO3MMNZLPVDKERA2D3HOPZMSXAK5VBBEM364TGS3A";
            this.LiquidityAssetId = 428917669;
            this.LiquidityWallet = "AIFU57RAPPX672WDLAUGZE46677XNGYQEX2KQM54DJZ4HUTQ264M2TALQA";
            this.LiquidityMinimum = 400000;
            this.DropTotal = 20000000;
            this.DropMinimum = 0;
            this.indexerUtils = indexerUtils;
        }

        public async Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts()
        {
            List<string> walletAddresses = (await this.FetchWalletAddresses()).ToList();
            walletAddresses.Remove(this.LiquidityWallet);
            walletAddresses.Remove(this.CreatorWallet);

            IEnumerable<(string, ulong)> liquidityAmounts = await this.GetLiquidityAmounts(walletAddresses);

            ulong liquidityTotal = (ulong)liquidityAmounts.Sum(la => (double)la.Item2);

            List<AirdropAmount> airdropAmounts = new List<AirdropAmount>();

            foreach ((string, long) liquidityAmount in liquidityAmounts)
            {
                ulong dropAmount = (ulong)(this.DropTotal * ((double)liquidityAmount.Item2 / (double)liquidityTotal));
                if (dropAmount > DropMinimum)
                {
                    airdropAmounts.Add(new AirdropAmount(liquidityAmount.Item1, this.AssetId, dropAmount));
                }
            }

            return airdropAmounts;
        }

        public Task<IEnumerable<(string, ulong)>> GetLiquidityAmounts(IEnumerable<string> walletAddresses)
        {
            ConcurrentBag<(string, ulong)> liquidityAmounts = new ConcurrentBag<(string, ulong)>();

            Parallel.ForEach(walletAddresses, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async walletAddress =>
            {
                Account account = await this.indexerUtils.GetAccount(walletAddress);
                ulong liquidityAmount = this.GetLiquidityAssetAmount(account.Assets);

                if (liquidityAmount > this.LiquidityMinimum)
                {
                    IEnumerable<Transaction> transactions = await this.indexerUtils.GetTransactions(walletAddress, this.LiquidityAssetId, afterTime: DateTime.Now.AddDays(-7), txType: TxType.Axfer);
                    ulong lowAmount = this.GetAssetLowest(walletAddress, liquidityAmount, transactions);
                    liquidityAmounts.Add((walletAddress, lowAmount));
                }
            });

            return Task.FromResult<IEnumerable<(string, ulong)>>(liquidityAmounts);
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
    }
}
