using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class ParlimentAowlPartnerFactory : AcornPartner
    {
        public ParlimentAowlPartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] {
                "AOWLLUX3BBLDV6KUZYQ7FBZTIWGWRRJO6B5XL2DFQ6WLITHUK26OO7IGMI",
                "AOWL3ZOC55JJDB5LU5CCIVK3WITVXMVZU6PV2CQQOLXUNCTOJQ6VJCPYAU",
                "AOWLDAJSAIHNR4J2TKB4U32Z73O7W52JGJUGUR5RW6ZH46G3ET4IFNNGQ4"};
            this.NumberOfWinners = 8;
            this.TotalWinnings = totalWinnings;
        }
    }
}
