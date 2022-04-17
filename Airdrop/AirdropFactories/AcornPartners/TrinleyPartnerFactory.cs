using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class TrinleyPartnerFactory : AcornPartner
    {
        public TrinleyPartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] {
                "KLKLHRAT525BOEPR72XMBIPDK77AI52TF6QC5LGNV2J2J3SCUGKDJVLCX4"};
            this.NumberOfWinners = 7;
            this.TotalWinnings = totalWinnings;
        }
    }
}
