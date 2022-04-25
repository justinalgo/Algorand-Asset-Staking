using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class AlgorillaPartnerFactory : AcornPartner
    {
        public AlgorillaPartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] { "PO4CEJB6IV2P5UACZ3P77KJCITMX2ZIT6RMW4WTX6JQGJYNJS6T5E4V27Q", "MPRRGD2IXHYNHRMOFD5AE6Y2KK6DL32GKDFIZG7SC6TYO6AKK7CZSSBKTA" };
            this.NumberOfWinners = 10;
            this.TotalWinnings = totalWinnings;
        }
    }
}
