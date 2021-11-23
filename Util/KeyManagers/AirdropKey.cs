using Algorand;
using Microsoft.Extensions.Configuration;

namespace Util.KeyManagers
{
    public class AirdropKey : IKeyManager
    {
        private readonly Account account;

        public AirdropKey(IConfiguration config)
        {
            this.account = new Account(config["airdropMnemonic"]);
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
