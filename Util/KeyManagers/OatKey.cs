using Algorand;
using Microsoft.Extensions.Configuration;

namespace Util.KeyManagers
{
    public class OatKey : IKeyManager
    {
        private readonly Account account;

        public OatKey(IConfiguration config)
        {
            this.account = new Account(config["OatMnemonic"]);
        }

        public Address GetAddress()
        {
            return this.account.Address;
        }

        public SignedTransaction SignTransaction(Transaction txn)
        {
            return this.account.SignTransaction(txn);
        }
    }
}
