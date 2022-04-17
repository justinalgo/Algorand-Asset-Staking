using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class MonstiPartnerFactory : AcornPartner
    {
        public MonstiPartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] {
                "KEFVKQAEKQBFHS5UJMC3JL7OSODFIEBYRAYXDXL5UL4WEBHSVMB2EBITP4"};
            this.NumberOfWinners = 5;
            this.TotalWinnings = totalWinnings;
        }
    }
}
