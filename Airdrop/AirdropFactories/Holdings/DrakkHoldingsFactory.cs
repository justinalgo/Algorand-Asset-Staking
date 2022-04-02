using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utils.Cosmos;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Holdings
{
    public class DrakkHoldingsFactory : RandHoldingsAirdropFactory
    {
        private readonly IIndexerUtils indexerUtils;
        private readonly ICosmos cosmos;
        private readonly ulong liquidityAssetId;

        private readonly string[] RevokedAddresses;

        public DrakkHoldingsFactory(IIndexerUtils indexerUtils, ICosmos cosmos, IHttpClientFactory httpClientFactory) : base(httpClientFactory.CreateClient())
        {
            this.DropAssetId = 560039769;
            this.Decimals = 6;
            this.CreatorAddresses = new string[] { "UXXYI4CPUIZ27UTWNL42VO7EG5LQGRQHLNKD2JIPVHEBZ7T7JXYOHAGW4A" };
            this.RevokedAddresses = new string[] { "7VGVH2G7R7MM7HNRV6AFIC7HP3WXIK6CZ42GGSK6LRRMREGNOQXRATBMLA" };
            this.indexerUtils = indexerUtils;
            this.cosmos = cosmos;
            this.liquidityAssetId = 572918686;
        }

        public override async Task<IEnumerable<Account>> FetchAccounts()
        {
            IEnumerable<Account> accounts = await this.indexerUtils.GetAccounts(this.DropAssetId, new ExcludeType[] { ExcludeType.CreatedAssets, ExcludeType.CreatedApps, ExcludeType.AppsLocalState });

            accounts = accounts.Where(a => !this.CreatorAddresses.Contains(a.Address));
            accounts = accounts.Where(a => !this.RevokedAddresses.Contains(a.Address));

            return accounts;
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
