using Algorand.V2.Indexer.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class StupidHorsePartnerFactory : AcornPartner
    {
        public StupidHorsePartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] {
                "GLOW7AKCAZXWQRPI6Q7OCVAO75H45AIYMTDEH3VNPETKYFXMNHAMQOVMS4",
                "STPD5WZ7DMF2RBBGROROWS6U2HNKC4SOHZXTFDTRIWHTXQ46TA7HU3A2SI"};
            this.AssetPrefix = "HORSE";
            this.NumberOfWinners = 2;
            this.TotalWinnings = totalWinnings;
        }
    }
}
