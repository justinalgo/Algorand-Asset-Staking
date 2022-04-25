using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class AlgoBullsPartnerFactory : AcornPartner
    {
        public AlgoBullsPartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] {
                "6S62ERMBGO2ZSSXWVIT6XM4E2P6JLS2PKPVBRJAGLOLOQCXCHMXHOQAUPU"};
            this.NumberOfWinners = 10;
            this.TotalWinnings = totalWinnings;
        }
    }
}
