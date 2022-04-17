using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class YieldlingPartnerFactory : AcornPartner
    {
        public YieldlingPartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] {
                "5DYIZMX7N4SAB44HLVRUGLYBPSN4UMPDZVTX7V73AIRMJQA3LKTENTLFZ4"};
            this.NumberOfWinners = 25;
            this.TotalWinnings = totalWinnings;
        }
    }
}
