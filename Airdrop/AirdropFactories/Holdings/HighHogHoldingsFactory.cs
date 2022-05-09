using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Holdings
{
    public class HighHogHoldingsFactory : RandHoldingsAirdropFactory
    {
        public HighHogHoldingsFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, IHttpClientFactory httpClientFactory) : base(indexerUtils, algodUtils, httpClientFactory.CreateClient())
        {
            this.DropAssetId = 455356741;
            this.Decimals = 0;
            this.CreatorAddresses = new string[] { "YZEDY575NGDQPBQT3FD5C6S56DFNNICIGN32X2GI6AC63MVOPZMWVOZY7Y" };
            this.AssetValue = 5000;
        }
    }
}
