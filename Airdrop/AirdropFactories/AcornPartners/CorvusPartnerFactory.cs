using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class CorvusPartnerFactory : AcornPartner
    {
        public CorvusPartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] { "GCMLRLVHQ7VBW7UBH26ZLPJVS2ZRBAUVQRMILPQRICFI6LO2G5QRKVKX44" };
            this.NumberOfWinners = 10;
            this.TotalWinnings = totalWinnings;
        }
    }
}
