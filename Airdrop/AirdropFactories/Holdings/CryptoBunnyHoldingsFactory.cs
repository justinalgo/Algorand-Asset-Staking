﻿using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Util;
using Util.Cosmos;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Holdings
{
    public class CryptoBunnyHoldingsFactory : HoldingsAirdropFactory
    {
        private readonly IIndexerUtils indexerUtils;
        private readonly ICosmos cosmos;

        public CryptoBunnyHoldingsFactory(IIndexerUtils indexerUtils, ICosmos cosmos)
        {
            this.AssetId = 329532956;
            this.Decimals = 0;
            this.CreatorAddresses = new string[] { "BNYSETPFTL2657B5RCSW64A3M766GYBVRV5ALOM7F7LIRUZKBEOGF6YSO4" };
            this.indexerUtils = indexerUtils;
            this.cosmos = cosmos;
        }

        public override async Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts()
        {
            IDictionary<ulong, ulong> assetValues = await this.FetchAssetValues();
            List<AirdropAmount> airdropAmounts = new List<AirdropAmount>();
            IEnumerable<Account> accounts = await this.FetchAccounts();

            Parallel.ForEach(accounts, new ParallelOptions { MaxDegreeOfParallelism = 10 }, account =>
            {
                IEnumerable<AssetHolding> assetHoldings = account.Assets;

                ulong amount = this.GetAssetHoldingsAmount(assetHoldings, assetValues);

                if (amount > 0)
                {
                    airdropAmounts.Add(new AirdropAmount(account.Address, this.AssetId, amount));
                }
            });

            return airdropAmounts;
        }

        public override async Task<IEnumerable<Account>> FetchAccounts()
        {
            IEnumerable<Account> accounts = await this.indexerUtils.GetAccounts(this.AssetId);

            return accounts;
        }

        public override async Task<IDictionary<ulong, ulong>> FetchAssetValues()
        {
            IEnumerable<AssetValue> assets = await cosmos.GetAssetValues("CryptoBunny");

            Dictionary<ulong, ulong> assetValues = assets.ToDictionary(av => av.AssetId, av => av.Value);

            return assetValues;
        }
    }
}
