using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class PyreneesPartnerFactory : AcornPartner
    {
        public PyreneesPartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] { "IEQGNR7DPQ26ZY7A22VHRTLHDL2PZ3XD5AUMMEEPYB6DUL6FSRN2NUUWPE" };
            this.NumberOfWinners = 10;
            this.TotalWinnings = totalWinnings;
        }
    }
}
