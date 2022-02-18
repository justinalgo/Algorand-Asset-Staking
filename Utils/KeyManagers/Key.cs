using Algorand;

namespace Utils.KeyManagers
{
    public class Key
    {
        private readonly Account account;

        public Key(string mnemonic)
        {
            this.account = new Account(mnemonic);
        }

        public Address GetAddress()
        {
            return this.account.Address;
        }

        public SignedTransaction SignTransaction(Transaction txn)
        {
            return this.account.SignTransaction(txn);
        }

        public override string ToString()
        {
            return this.GetAddress().EncodeAsString();
        }
    }
}
