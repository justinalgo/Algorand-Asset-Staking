using Algorand;
using Microsoft.Extensions.Configuration;

namespace Util.KeyManagers
{
    public class ShrimpKey : IKeyManager
    {
        private readonly Account account;

        public ShrimpKey(IConfiguration config)
        {
            this.account = new Account(config["lingLingMnemonic"]);
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
