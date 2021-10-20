using Algorand.V2.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Util;

namespace Airdrop
{
    public class ShrimpAirdropFactory : AirdropFactory
    {
        private readonly IApi _api;

        public ShrimpAirdropFactory(IApi api) : base(360019122) {
            this._api = api;
        }

        public override IEnumerable<AirdropAmount> FetchAirdropAmounts(IDictionary<long, long> assetValues)
        {
            List<AirdropAmount> airdropAmounts = new List<AirdropAmount>();
            IEnumerable<string> walletAddresses = this.FetchWalletAddresses();

            foreach (string walletAddress in walletAddresses)
            {
                IEnumerable<AssetHolding> assetHoldings = this._api.GetAssetsByAddress(walletAddress);
                long amount = this.GetAirdropAmount(assetHoldings, assetValues);
                airdropAmounts.Add(new AirdropAmount(walletAddress, amount));
            }

            return airdropAmounts;
        }

        public override IEnumerable<string> FetchWalletAddresses()
        {
            IEnumerable<string> walletAddresses = this._api.GetWalletAddressesWithAsset(this.AssetId);

            return walletAddresses;
        }

        public override IDictionary<long, long> GetAssetValues()
        {
            List<AssetValue> values = JsonConvert.DeserializeObject<List<AssetValue>>(File.ReadAllText("C:/Users/ParkG/source/repos/Airdrop/Airdrop/ShrimpValues.json"));

            Dictionary<long, long> assetValues = values.ToDictionary(av => av.AssetId, av => av.Value);

            return assetValues;
        }

        private long GetAirdropAmount(IEnumerable<AssetHolding> assetHoldings, IDictionary<long, long> assetValues)
        {
            long airdropAmount = 0;

            foreach (AssetHolding miniAssetHolding in assetHoldings)
            {
                if (miniAssetHolding.AssetId.HasValue &&
                    miniAssetHolding.Amount.HasValue &&
                    miniAssetHolding.Amount > 0 &&
                    assetValues.ContainsKey(miniAssetHolding.AssetId.Value))
                {
                    airdropAmount += (long)miniAssetHolding.Amount.Value;
                }
            }

            return airdropAmount;
        }
    }
}
