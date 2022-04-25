using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class AlgoSaiyansPartnerFactory : AcornPartner
    {
        public AlgoSaiyansPartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] { "SXLGJQWTJA46WSE6UITNJSHA5PWWEP6WTWUF3GBOVRJ4W2E2OUOQL52DKA" };
            this.NumberOfWinners = 5;
            this.TotalWinnings = totalWinnings;
        }
    }
}
