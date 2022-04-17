using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.Liquidity
{
    public class AlchemonLiquidityFactory : LiquidityAirdropFactory
    {
        public AlchemonLiquidityFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils) : base(indexerUtils, algodUtils)
        {
            this.DropAssetId = 310014962;
            this.Decimals = 0;
            this.LiquidityAssetId = 552701368;
            this.LiquidityWallet = "EJGN54S3OSQXDX5NYOGYZBGLIZZEKQSROO3AXKX2WPJ2CRMAW57YMDXWWE";
            this.LiquidityMinimum = 0;
            this.DropTotal = 12500;
            this.DropMinimum = 0;
            this.RevokedAddresses = new string[] { "EJGN54S3OSQXDX5NYOGYZBGLIZZEKQSROO3AXKX2WPJ2CRMAW57YMDXWWE" };
            this.AfterTime = DateTime.Now.AddDays(-6.5);
        }
    }
}
