using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Utils.Indexer
{
    public interface IIndexerUtils
    {
        Task<Account> GetAccount(string address);
        Task<IEnumerable<Account>> GetAccounts(ulong assetId);
        Task<IEnumerable<Account>> GetAccounts(ulong assetId, params ulong[] assetIds);
        Task<IEnumerable<Asset>> GetAsset(IEnumerable<ulong> assetIds);
        Task<Asset> GetAsset(ulong assetId);
        Task<IEnumerable<Transaction>> GetTransactions(string address, ulong assetId, AddressRole? addressRole = null, TxType? txType = null, ulong? currencyGreaterThan = null, ulong? currencyLessThan = null, ulong? minRound = null, DateTimeOffset? afterTime = null);
        Task<IEnumerable<string>> GetWalletAddresses(string address, ulong assetId, AddressRole? addressRole = null, TxType? txType = null, ulong? currencyGreaterThan = null, ulong? currencyLessThan = null, ulong? minRound = null, DateTimeOffset? afterTime = null);
        Task<IEnumerable<string>> GetWalletAddresses(ulong assetId);
        Task<IEnumerable<string>> GetWalletAddresses(ulong assetId, params ulong[] assetIds);
    }
}