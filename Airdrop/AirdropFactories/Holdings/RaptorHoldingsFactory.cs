using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Cosmos;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Holdings
{
    public class RaptorHoldingsFactory : RandHoldingsAirdropFactory
    {
        private readonly ICosmos cosmos;

        public RaptorHoldingsFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ICosmos cosmos, IHttpClientFactory httpClientFactory) : base(indexerUtils, algodUtils, httpClientFactory.CreateClient())
        {
            this.DropAssetId = 426980914;
            this.Decimals = 2;
            this.CreatorAddresses = new string[] {
                "EYERHFMTLLBVVDV4RKGKSTMWU4QZ7IG5HUEXQYO3UVX4ABQ5MDNJIKN7HQ",
                "MOON3QNZT5XBPIYLMASRS5RSFRVVZF4SAMG4Q22KQFRLWKXXW2S2MY2WD4",
                "OPBIE5S3IUKLNFU6C2DK5B3FTYD7W7BMVDTNMTFFLKVQXPUUNAFBHZHVBY",
                "BEEE3WGLXN6QD62D3LVB67DF5LESMIS6FVD4QTMOV3325OM24CDQKBWI6U",
            };
            this.cosmos = cosmos;
        }

        public override async Task<IDictionary<ulong, ulong>> FetchAssetValues()
        {
            IEnumerable<AssetValue> values = await cosmos.GetAssetValues("RaptorCoin");

            Dictionary<ulong, ulong> assetValues = values.ToDictionary(av => av.AssetId, av => (ulong)(av.Value * Math.Pow(10, this.Decimals)));

            return assetValues;
        }
    }
}
