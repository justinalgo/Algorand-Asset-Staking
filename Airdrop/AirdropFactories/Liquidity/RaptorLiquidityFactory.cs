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
    public class RaptorLiquidityFactory : LiquidityAirdropFactory
    {
        public RaptorLiquidityFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils) : base(indexerUtils, algodUtils)
        {
            this.DropAssetId = 426980914;
            this.Decimals = 2;
            this.LiquidityAssetId = 428917669;
            this.LiquidityWallet = "AIFU57RAPPX672WDLAUGZE46677XNGYQEX2KQM54DJZ4HUTQ264M2TALQA";
            this.LiquidityMinimum = 400000;
            this.DropTotal = 20000000;
            this.DropMinimum = 0;
            this.RevokedAddresses = new string[] { "AIFU57RAPPX672WDLAUGZE46677XNGYQEX2KQM54DJZ4HUTQ264M2TALQA", "SBKN5JI72DS4USUUIFO3MMNZLPVDKERA2D3HOPZMSXAK5VBBEM364TGS3A" };
            this.AfterTime = DateTime.Now.AddDays(-7);
        }
    }
}
