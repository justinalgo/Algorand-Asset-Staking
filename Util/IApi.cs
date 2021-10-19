using Algorand;
using Algorand.V2.Model;
using System.Collections.Generic;

namespace Util
{
    public interface IApi
    {
        Algorand.V2.Model.Account GetAccountByAddress(string walletAddress);
        Asset GetAssetById(long assetId);
        IEnumerable<Asset> GetAssetById(IEnumerable<long> assetIds);
        long GetAssetDecimals(int assetId);
        IEnumerable<AssetHolding> GetAssetsByAddress(string walletAddress);
        IEnumerable<string> GetWalletAddressesWithAsset(long assetId);
        IEnumerable<string> GetWalletAddressesWithAsset(long assetId, params long[] assetIds);
        void SubmitTransactions(IEnumerable<SignedTransaction> signedTxns);
    }
}