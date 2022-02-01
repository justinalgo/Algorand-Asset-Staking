﻿using Algorand.V2.Indexer.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Airdrop.AirdropFactories
{
    public interface IAirdropFactory
    {
        public ulong AssetId { get; set; }
        public ulong Decimals { get; set; }
        public Task<IEnumerable<Account>> FetchAccounts();
        public Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts();
    }
}
