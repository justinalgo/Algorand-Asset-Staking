using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class SwappyPartnerFactory : AcornPartner
    {
        public SwappyPartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] {
                "GANGDMU6TWCMQPBLXBCW6ZEBZJ35VK2665CASIGV2ARSVZB7VIOKWAA5UU",};
            this.NumberOfWinners = 50;
            this.TotalWinnings = totalWinnings;
        }
    }
}
