using Algorand.V2.Indexer.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class BananaMintPartnerFactory : AcornPartner
    {
        public BananaMintPartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] {
                "NV7D4EFKO5FRXEHRVMEP3LDG6IACFQVJXYYG6KJAGXW2JRBKW3Y7UNQE2Y"};
            this.AssetPrefix = "BNAMNT";
            this.NumberOfWinners = 2;
            this.TotalWinnings = totalWinnings;
        }
    }
}
