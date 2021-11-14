using Algorand;

namespace Util.KeyManagers
{
    public interface IKeyManager
    {
        public Address GetAddress();
        public SignedTransaction SignTransaction(Transaction txn);
    }
}
