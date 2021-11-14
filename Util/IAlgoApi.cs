using Algorand;
using Algorand.V2.Model;
using System.Collections.Generic;
using Util.Cosmos;

namespace Util
{
    public interface IAlgoApi
    {
        Algorand.V2.Model.Account GetAccountByAddress(string walletAddress);
        Asset GetAssetById(long assetId);
        IEnumerable<Asset> GetAssetById(IEnumerable<long> assetIds);
        long GetAssetDecimals(int assetId);
        IEnumerable<AssetHolding> GetAssetsByAddress(string walletAddress);
        IEnumerable<string> GetWalletAddressesWithAsset(long assetId);
        IEnumerable<string> GetWalletAddressesWithAsset(long assetId, params long[] assetIds);
        TransactionParametersResponse GetTransactionParams();
        IEnumerable<AssetValue> GetAccountAssetValues(string walletAddress, string startsWithString = "", string projectId = null, long? value = null);
        TransactionsResponse GetAssetTransactions(string senderAddress, long assetId, long minRound, long limit = 100, string next = null);
        IEnumerable<string> GetAddressesSent(string senderAddress, long assetId, long minRound, long limit = 100);
        PendingTransactionResponse SubmitTransactionWait(SignedTransaction signedTxn);
        PostTransactionsResponse SubmitTransaction(SignedTransaction signedTxn);
        NodeStatusResponse GetStatus();
        long? GetLastRound();
        NodeStatusResponse GetStatusAfterRound(long round);
    }
}