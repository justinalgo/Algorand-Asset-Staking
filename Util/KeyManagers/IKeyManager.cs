using Algorand;

namespace Util
{
    public interface IKeyManager
    {
        public Address GetAddress();
        public SignedTransaction SignTransaction(Transaction txn);
    }
}
