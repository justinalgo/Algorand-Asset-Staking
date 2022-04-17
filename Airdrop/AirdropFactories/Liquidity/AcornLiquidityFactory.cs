using Algorand.V2.Algod.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Liquidity
{
    public class AcornLiquidityFactory : LiquidityAirdropFactory
    {
        public AcornLiquidityFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils) : base(indexerUtils, algodUtils)
        {
            this.DropAssetId = 226265212;
            this.Decimals = 0;
            this.LiquidityAssetId = 552642388;
            this.LiquidityWallet = "H3DSZ4DXLJQ7OXG3NQUSGS2NXDNQG4BVZRWJWGGWDOVORHRTLE2IVJYGT4";
            this.LiquidityMinimum = 0;
            this.DropTotal = 1000000;
            this.DropMinimum = 0;
            this.RevokedAddresses = new string[] { "GOGC4QDPXDK3WEVO2BYBS3LGSMTWDMQHDOO5KRJ7HLT6HDMI5FX7MLUMRA", "H3DSZ4DXLJQ7OXG3NQUSGS2NXDNQG4BVZRWJWGGWDOVORHRTLE2IVJYGT4" };
            this.AfterTime = DateTime.Now.AddDays(-6.5);
        }
    }
}
