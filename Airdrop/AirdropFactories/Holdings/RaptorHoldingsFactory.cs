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
    public class RaptorHoldingsFactory : ExchangeHoldingsAirdropFactory
    {

        public RaptorHoldingsFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, IHttpClientFactory httpClientFactory) : base(indexerUtils, algodUtils, httpClientFactory.CreateClient())
        {
            this.DropAssetId = 426980914;
            this.Decimals = 2;
            this.CreatorAddresses = new string[] {
                "EYERHFMTLLBVVDV4RKGKSTMWU4QZ7IG5HUEXQYO3UVX4ABQ5MDNJIKN7HQ",
            };
            this.AssetValue = 1200000;
            this.SearchRand = true;
        }
    }
}
