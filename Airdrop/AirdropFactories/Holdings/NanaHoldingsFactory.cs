using Algorand.V2.Indexer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Util;
using Util.Cosmos;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Holdings
{
    public class NanaHoldingsFactory : RandHoldingsAirdropFactory
    {
        private readonly IIndexerUtils indexerUtils;
        private readonly ICosmos cosmos;

        public NanaHoldingsFactory(IIndexerUtils indexerUtils, ICosmos cosmos, IHttpClientFactory httpClientFactory) : base(httpClientFactory.CreateClient())
        {
            this.DropAssetId = 418706707;
            this.Decimals = 0;
            this.CreatorAddresses = new string[] { "NV7D4EFKO5FRXEHRVMEP3LDG6IACFQVJXYYG6KJAGXW2JRBKW3Y7UNQE2Y" };
            this.indexerUtils = indexerUtils;
            this.cosmos = cosmos;
        }

        public override async Task<IEnumerable<Account>> FetchAccounts()
        {
            IEnumerable<Account> accounts = await this.indexerUtils.GetAccounts(this.DropAssetId);

            return accounts;
        }

        public override async Task<IDictionary<ulong, ulong>> FetchAssetValues()
        {
            IEnumerable<AssetValue> values = await cosmos.GetAssetValues("NanaCoin");

            Dictionary<ulong, ulong> assetValues = values.ToDictionary(av => av.AssetId, av => av.Value);

            return assetValues;
        }
    }
}
