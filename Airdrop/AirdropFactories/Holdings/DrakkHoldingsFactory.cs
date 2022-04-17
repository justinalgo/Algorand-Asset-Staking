using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Cosmos;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Holdings
{
    public class DrakkHoldingsFactory : RandHoldingsAirdropFactory
    {
        private readonly ICosmos cosmos;
        private readonly ulong liquidityAssetId;

        public DrakkHoldingsFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ICosmos cosmos, IHttpClientFactory httpClientFactory) : base(indexerUtils, algodUtils, httpClientFactory.CreateClient())
        {
            this.DropAssetId = 560039769;
            this.Decimals = 6;
            this.CreatorAddresses = new string[] { "UXXYI4CPUIZ27UTWNL42VO7EG5LQGRQHLNKD2JIPVHEBZ7T7JXYOHAGW4A" };
            this.RevokedAddresses = new string[] { "7VGVH2G7R7MM7HNRV6AFIC7HP3WXIK6CZ42GGSK6LRRMREGNOQXRATBMLA", "UXXYI4CPUIZ27UTWNL42VO7EG5LQGRQHLNKD2JIPVHEBZ7T7JXYOHAGW4A" };
            this.cosmos = cosmos;
            this.liquidityAssetId = 572918686;
        }

        public override async Task<IDictionary<ulong, ulong>> FetchAssetValues()
        {
            IEnumerable<AssetValue> assets = await cosmos.GetAssetValues("Drakk");

            Dictionary<ulong, ulong> assetValues = assets.ToDictionary(av => av.AssetId, av => (ulong) (av.Value * Math.Pow(10, this.Decimals)));
            assetValues[this.liquidityAssetId] = 2;

            return assetValues;
        }
    }
}
