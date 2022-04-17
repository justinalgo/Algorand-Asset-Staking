using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class TinyWhalesPartnerFactory : AcornPartner
    {
        public TinyWhalesPartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] {
                "WALEJYNKT5LDSHBR43Y2PCDYCIUA4LQ4ZCMJ2MAYODLJG5YUVAAP4YX7UQ",
                "WALEHCLT5GH4PXZAJHFAJIKTBOVOJVQCXXNIZAQICD27MGNWPDMV2TK3CI",
                "WALEHTQCNBJ7JTIG5JREIIC3HMZVDYKMM5L6DHHMVKU4DOSLRQOBHO45PQ" };
            this.RevokedAssets = new ulong[]
            {
                513799026, 516993196, 516995986, 517005315, 517007356, 517011087, 517016841, 517018556, 517034102, 517035626
            };
            this.NumberOfWinners = 10;
            this.TotalWinnings = totalWinnings;
        }
    }
}
