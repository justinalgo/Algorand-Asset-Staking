﻿using Algorand;
using Algorand.V2.Model;
using System.Collections.Generic;

namespace Api
{
    public interface IApiUtil
    {
        Algorand.V2.Model.Account GetAccountByAddress(string walletAddress);
        Asset GetAssetById(long assetId);
        long GetAssetDecimals(int assetId);
        List<AssetHolding> GetAssetsByAddress(string walletAddress);
        IEnumerable<string> GetWalletAddressesWithAsset(long assetId);
        IEnumerable<string> GetWalletAddressesWithAsset(long assetId, params long[] assetIds);
        PendingTransactionResponse SubmitTransaction(SignedTransaction signedTxn);
    }
}