using Algorand.V2.Algod.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Holdings
{
    public class MantisHoldingsFactory : HoldingsAirdropFactory
    {
        public MantisHoldingsFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils) : base(indexerUtils, algodUtils)
        {
            this.DropAssetId = 714976897;
            this.Decimals = 8;
            this.CreatorAddresses = new string[] {
                "AII4BCB5GN6YMSP7TNWMQRM4X2SVZY7AUMKQAGLMZBKZFC2LUWAER2CVOQ",
            };
            this.AssetValue = 285;
        }

        public override async Task<IDictionary<ulong, ulong>> FetchAssetValues()
        {
            Dictionary<ulong, ulong> assetValues = new Dictionary<ulong, ulong>();

            foreach (string creatorAddress in this.CreatorAddresses)
            {
                Account account = await this.AlgodUtils.GetAccount(creatorAddress);
                var assets = account.CreatedAssets;

                if (this.RevokedAddresses != null)
                {
                    foreach (var asset in assets)
                    {
                        if (!this.RevokedAssets.Contains(asset.Index))
                        {
                            assetValues.Add(asset.Index, this.AssetValue);
                        }
                    }
                }
                else
                {
                    foreach (var asset in assets)
                    {
                        assetValues.Add(asset.Index, this.AssetValue);
                    }
                }
            }

            return assetValues;
        }
    }
}
