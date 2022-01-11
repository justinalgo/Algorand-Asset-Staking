using Algorand.V2.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Util;
using Util.Cosmos;

namespace Airdrop.AirdropFactories.Holdings
{
    public class AowlHoldingsFactory : IHoldingsAirdropFactory
    {
        public string[] CreatorAddresses { get; set; }
        public long AssetId { get; set; }
        public long Decimals { get; set; }
        private readonly IAlgoApi api;
        private readonly ICosmos cosmos;

        public AowlHoldingsFactory(IAlgoApi api, ICosmos cosmos)
        {
            this.AssetId = -1;
            this.Decimals = -1;
            this.CreatorAddresses = new string[] {
                "AOWLLUX3BBLDV6KUZYQ7FBZTIWGWRRJO6B5XL2DFQ6WLITHUK26OO7IGMI",
                "AOWL3ZOC55JJDB5LU5CCIVK3WITVXMVZU6PV2CQQOLXUNCTOJQ6VJCPYAU",
                "AOWLDAJSAIHNR4J2TKB4U32Z73O7W52JGJUGUR5RW6ZH46G3ET4IFNNGQ4"
            };
            this.api = api;
            this.cosmos = cosmos;
        }

        public Task<IEnumerable<AirdropAmount>> FetchAirdropAmounts()
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<long, long>> FetchAssetValues()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> FetchWalletAddresses()
        {
            throw new NotImplementedException();
        }

        public long GetAssetHoldingsAmount(IEnumerable<AssetHolding> assetHoldings, IDictionary<long, long> assetValues)
        {
            throw new NotImplementedException();
        }
    }
}
