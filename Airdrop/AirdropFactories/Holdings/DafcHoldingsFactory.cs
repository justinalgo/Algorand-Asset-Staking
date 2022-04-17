using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Holdings
{
    public class DafcHoldingsFactory : RandHoldingsAirdropFactory
    {
        public DafcHoldingsFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, IHttpClientFactory httpClientFactory) : base(indexerUtils, algodUtils, httpClientFactory.CreateClient())
        {
            this.DropAssetId = 624128801;
            this.Decimals = 0;
            this.CreatorAddresses = new string[] { "DAFCFSH5IFPO4DWDUHHZFV6UESAJB4IJRZ3R4LXATMDHQ2FFIDEGXBBPHA" };
            this.AssetValue = 1;
        }
    }
}
