using Algorand;
using Algorand.V2.Model;
using System.Collections.Generic;

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
        PendingTransactionResponse SubmitTransaction(SignedTransaction signedTxn);
        TransactionParametersResponse GetTransactionParams();
        IEnumerable<AssetValue> GetAccountAssetValues(string walletAddress, string startsWithString = "", string projectId = null, long? value = null);
    }
}