using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class FlemishPartnerFactory : AcornPartner
    {
        public FlemishPartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] {
                "PIIQQI6WFNT3S6VEMNTQZNWLDUYXUEWRLZLTBWMJABEGPUZXML2Y5SKC6A"};
            this.NumberOfWinners = 5;
            this.TotalWinnings = totalWinnings;
        }
    }
}
