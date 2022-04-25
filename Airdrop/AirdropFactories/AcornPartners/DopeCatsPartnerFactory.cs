using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class DopeCatsPartnerFactory : AcornPartner
    {
        public DopeCatsPartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] { "6S62ERMBGO2ZSSXWVIT6XM4E2P6JLS2PKPVBRJAGLOLOQCXCHMXHOQAUPU", "DAFCFSH5IFPO4DWDUHHZFV6UESAJB4IJRZ3R4LXATMDHQ2FFIDEGXBBPHA" };
            this.NumberOfWinners = 10;
            this.TotalWinnings = totalWinnings;
        }
    }
}
