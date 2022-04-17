using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class AlgoWhalesPartnerFactory : AcornPartner
    {
        public AlgoWhalesPartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] {
                "7666EISCAZ6NSIB5MR633UKU5PU4LF2KGDAN2U6AMSQ7EPVJWAITX5EG7M"};
            this.RevokedAssets = new ulong[] {
                343613313, 463356107, 463510370, 463544807, 463590949, 463622110
            };
            this.NumberOfWinners = 10;
            this.TotalWinnings = totalWinnings;
        }
    }
}
