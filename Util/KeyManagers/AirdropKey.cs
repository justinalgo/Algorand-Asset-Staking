using Algorand;
using Microsoft.Extensions.Configuration;
using System;

namespace Util
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
