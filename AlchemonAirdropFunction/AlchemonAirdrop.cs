using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Airdrop;
using Airdrop.AirdropFactories.Holdings;
using Airdrop.AirdropFactories.Liquidity;
using Algorand;
using Algorand.Client;
using Algorand.V2.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Util;
using Util.KeyManagers;
using Transaction = Algorand.Transaction;

namespace AlchemonAirdropFunction
{
    public class AlchemonAirdrop
    {
        private readonly IAlgoApi api;
        private readonly IKeyManager keyManager;
        private readonly IHoldingsAirdropFactory holdingsAirdropFactory;
        private readonly ILiquidityAirdropFactory liquidityAirdropFactory;

        public AlchemonAirdrop(
            IAlgoApi api, 
            IKeyManager keyManager, 
            IHoldingsAirdropFactory holdingsAirdropFactory,
            ILiquidityAirdropFactory liquidityAirdropFactory)
        {
            this.api = api;
            this.keyManager = keyManager;
            this.holdingsAirdropFactory = holdingsAirdropFactory;
            this.liquidityAirdropFactory = liquidityAirdropFactory;
        }

        //0 0 16 * * Sat
        [FunctionName("AlchemonHoldingsAirdrop")]
        public async Task RunHoldingsAirdrop([TimerTrigger("0 0 16 * * Sat")]TimerInfo myTimer, ILogger log)
        {
            IEnumerable<AirdropAmount> amounts = await holdingsAirdropFactory.FetchAirdropAmounts();

            foreach (AirdropAmount amt in amounts)
            {
                log.LogInformation($"{amt.Wallet} : {amt.Amount}");
            }

            log.LogInformation($"Total airdrop amount: {amounts.Sum(a => a.Amount)}");
            log.LogInformation($"Number of wallets: {amounts.Count()}");

            long lastRound = api.GetLastRound().Value;
            log.LogInformation($"Round start: {lastRound}");

            Parallel.ForEach<AirdropAmount>(amounts, airdropAmount =>
            {
                try
                {
                    TransactionParametersResponse transactionParameters = api.GetTransactionParams();

                    Address address = new Address(airdropAmount.Wallet);

                    Transaction txn = Utils.GetTransferAssetTransaction(
                            keyManager.GetAddress(),
                            address,
                            airdropAmount.AssetId,
                            (ulong)airdropAmount.Amount,
                            transactionParameters
                        );

                    SignedTransaction stxn = keyManager.SignTransaction(txn);

                    PostTransactionsResponse resp = api.SubmitTransaction(stxn);
                    log.LogInformation($"{airdropAmount.Wallet} : {airdropAmount.Amount} with TxId: {resp.TxId}");
                }
                catch (ApiException ex)
                {
                    log.LogError("ApiException on {airdropAmount.Wallet}");
                    log.LogError($"Error code: {ex.ErrorCode}; Error content: {ex.ErrorCode}");
                }
                catch (ArgumentException)
                {
                    log.LogError($"{airdropAmount.Wallet} is an invalid address");
                }
            });

            api.GetStatusAfterRound(api.GetLastRound().Value + 5);

            IEnumerable<string> walletAddresses = api.GetAddressesSent(
                keyManager.GetAddress().EncodeAsString(),
                holdingsAirdropFactory.AssetId,
                lastRound
            );

            if (amounts.Count() != walletAddresses.Count())
            {
                foreach (AirdropAmount amount in amounts)
                {
                    if (!walletAddresses.Contains(amount.Wallet))
                    {
                        log.LogError($"Failed to drop: {amount.Wallet}");
                    }
                }
            }
            else
            {
                log.LogInformation("All addresses dropped successfully!");
            }
        }

        //0 0 17 * * Sat
        [FunctionName("AlchemonLiquidityAirdrop")]
        public async Task RunLiquidityAirdrop([TimerTrigger("0 0 17 * * Sat")] TimerInfo myTimer, ILogger log)
        {
            IEnumerable<AirdropAmount> amounts = await holdingsAirdropFactory.FetchAirdropAmounts();

            foreach (AirdropAmount amt in amounts)
            {
                log.LogInformation($"{amt.Wallet} : {amt.Amount}");
            }

            log.LogInformation($"Total airdrop amount: {amounts.Sum(a => a.Amount)}");
            log.LogInformation($"Number of wallets: {amounts.Count()}");

            long lastRound = api.GetLastRound().Value;
            log.LogInformation($"Round start: {lastRound}");

            Parallel.ForEach<AirdropAmount>(amounts, airdropAmount =>
            {
                try
                {
                    TransactionParametersResponse transactionParameters = api.GetTransactionParams();

                    Address address = new Address(airdropAmount.Wallet);

                    Transaction txn = Utils.GetTransferAssetTransaction(
                            keyManager.GetAddress(),
                            address,
                            airdropAmount.AssetId,
                            (ulong)airdropAmount.Amount,
                            transactionParameters
                        );

                    SignedTransaction stxn = keyManager.SignTransaction(txn);

                    PostTransactionsResponse resp = api.SubmitTransaction(stxn);
                    log.LogInformation($"{airdropAmount.Wallet} : {airdropAmount.Amount} with TxId: {resp.TxId}");
                }
                catch (ApiException ex)
                {
                    log.LogError("ApiException on {airdropAmount.Wallet}");
                    log.LogError($"Error code: {ex.ErrorCode}; Error content: {ex.ErrorCode}");
                }
                catch (ArgumentException)
                {
                    log.LogError($"{airdropAmount.Wallet} is an invalid address");
                }
            });

            api.GetStatusAfterRound(api.GetLastRound().Value + 5);

            IEnumerable<string> walletAddresses = api.GetAddressesSent(
                keyManager.GetAddress().EncodeAsString(),
                holdingsAirdropFactory.AssetId,
                lastRound
            );

            if (amounts.Count() != walletAddresses.Count())
            {
                foreach (AirdropAmount amount in amounts)
                {
                    if (!walletAddresses.Contains(amount.Wallet))
                    {
                        log.LogError($"Failed to drop: {amount.Wallet}");
                    }
                }
            }
            else
            {
                log.LogInformation("All addresses dropped successfully!");
            }
        }
    }
}
