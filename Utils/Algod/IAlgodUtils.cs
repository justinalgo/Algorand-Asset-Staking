using Algorand;
using Algorand.V2.Algod.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Utils.Algod
{
    public interface IAlgodUtils
    {
        Task<ulong> GetLastRound();
        Task<NodeStatusResponse> GetStatus();
        Task<NodeStatusResponse> GetStatusAfterRound(ulong round);
        Task<PostTransactionsResponse> SubmitTransaction(SignedTransaction signedTransaction);
        Task<TransactionParametersResponse> GetTransactionParams();
        Task<IEnumerable<string>> SubmitSignedTransactions(IEnumerable<SignedTransaction> signedTransactions);
        Task<Algorand.V2.Algod.Model.Account> GetAccount(string address);
    }
}