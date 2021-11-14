using Algorand;
using Microsoft.Extensions.Configuration;

namespace Util.KeyManagers
{
    public class MafialgoKey : IKeyManager
    {
        private readonly Account account;

        public MafialgoKey(IConfiguration config)
        {
            this.account = new Account(config["MafialgoMnemonic"]);
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
