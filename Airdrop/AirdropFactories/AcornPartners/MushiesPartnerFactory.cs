using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class MushiesPartnerFactory : AcornPartner
    {
        public MushiesPartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] {
                "NFZLQZAD3M3IGTF5DK2JENEAHOBP6V656P5S2EJKIBOI2KIV3VAJFNDRKI",
                "UWDYKPFNMTPDE64TLXVX2NQIVC4LP44XEZFF3KXDAVC63FPDAR3SR3AFP4"};
            this.NumberOfWinners = 5;
            this.TotalWinnings = totalWinnings;
        }
    }
}
