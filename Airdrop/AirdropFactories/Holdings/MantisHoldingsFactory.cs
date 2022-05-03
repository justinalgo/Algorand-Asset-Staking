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
                "SLVMSY7ENGPNQOW4FPABNTEB3EBZMNUBKTDWBWWST76XWNFO5TQGLRS3SA",
                "GLDMHYDO2XRYZRGZMJ5TEZRN345FL3KPURAT3HGZB6TGGBWRR7HGMPZZJE",
                "DMAN364FAMQWALOM7VIHTDGL7356MCAWAN2H23BJ7PL275THG3WGPX3GDE"
            };
            this.AssetValue = 28500000000;
        }

        public override async Task<IDictionary<ulong, ulong>> FetchAssetValues()
        {
            Dictionary<ulong, ulong> assetValues = new Dictionary<ulong, ulong>();

            await AssetHelper(assetValues, this.CreatorAddresses[0], this.AssetValue);
            await AssetHelper(assetValues, this.CreatorAddresses[1], 6 * this.AssetValue);
            await AssetHelper(assetValues, this.CreatorAddresses[2], 13 * this.AssetValue);
            await AssetHelper(assetValues, this.CreatorAddresses[3], 27 * this.AssetValue);

            return assetValues;
        }

        private async Task AssetHelper(IDictionary<ulong, ulong> assetValues, string wallet, ulong value)
        {
            Account account = await this.AlgodUtils.GetAccount(wallet);
            var assets = account.CreatedAssets;

            if (this.RevokedAddresses != null)
            {
                foreach (var asset in assets)
                {
                    if (!this.RevokedAssets.Contains(asset.Index))
                    {
                        assetValues.Add(asset.Index, value);
                    }
                }
            }
            else
            {
                foreach (var asset in assets)
                {
                    assetValues.Add(asset.Index, value);
                }
            }
        }
    }
}
