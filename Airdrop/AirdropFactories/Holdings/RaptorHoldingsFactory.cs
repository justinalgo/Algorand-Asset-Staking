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
    public class RaptorHoldingsFactory : RandHoldingsAirdropFactory
    {
        private readonly IIndexerUtils indexerUtils;
        private readonly ICosmos cosmos;

        public RaptorHoldingsFactory(IIndexerUtils indexerUtils, ICosmos cosmos, IHttpClientFactory httpClientFactory) : base(httpClientFactory.CreateClient())
        {
            this.DropAssetId = 426980914;
            this.Decimals = 2;
            this.CreatorAddresses = new string[] { 
                "EYERHFMTLLBVVDV4RKGKSTMWU4QZ7IG5HUEXQYO3UVX4ABQ5MDNJIKN7HQ",
                "MOON3QNZT5XBPIYLMASRS5RSFRVVZF4SAMG4Q22KQFRLWKXXW2S2MY2WD4",
                "OPBIE5S3IUKLNFU6C2DK5B3FTYD7W7BMVDTNMTFFLKVQXPUUNAFBHZHVBY",
                "BEEE3WGLXN6QD62D3LVB67DF5LESMIS6FVD4QTMOV3325OM24CDQKBWI6U",
            };
            this.indexerUtils = indexerUtils;
            this.cosmos = cosmos;
        }

        public override async Task<IDictionary<ulong, ulong>> FetchAssetValues()
        {
            IEnumerable<AssetValue> values = await cosmos.GetAssetValues("RaptorCoin", "MoonDude", "Numbers", "BuzzyBees");

            Dictionary<ulong, ulong> assetValues = values.ToDictionary(av => av.AssetId, av => av.Value);

            return assetValues;
        }

        public override async Task<IEnumerable<Account>> FetchAccounts()
        {
            IEnumerable<Account> accounts = await this.indexerUtils.GetAccounts(this.DropAssetId);

            return accounts;
        }
    }
}
