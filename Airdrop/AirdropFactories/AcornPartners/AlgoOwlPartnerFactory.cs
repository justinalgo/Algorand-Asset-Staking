using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class AlgoOwlPartnerFactory : AcornPartner
    {
        public AlgoOwlPartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] {
                "OWLETILXSEP4INXUQ3XTKRJX4ATYBHOFRQ6THSTCGS7ZFCINAEL4B4F7CQ"};
            this.NumberOfWinners = 2;
            this.TotalWinnings = totalWinnings;

        }
    }
}
