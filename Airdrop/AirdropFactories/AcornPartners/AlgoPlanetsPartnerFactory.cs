using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class AlgoPlanetsPartnerFactory : AcornPartner
    {
        public AlgoPlanetsPartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] {
                "PLNTKFIP4UOGJARQMEXQYHBQPJURARFRLTR6AEGWG6SPFZLFOZHYYXCAV4"};
            this.NumberOfWinners = 25;
            this.TotalWinnings = totalWinnings;
        }
    }
}
