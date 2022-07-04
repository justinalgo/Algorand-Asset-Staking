using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class GoannaPartnerFactory : AcornPartner
    {

        public GoannaPartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] { "D5J7H7PIYKLY2U6A5OFUAC7GQHTHSXXNX65DSD3CJYPBV2MVK6NTNW44CA"};
            this.NumberOfWinners = 1;
            this.TotalWinnings = totalWinnings;
            this.AssetPrefix = "goan";
        }
    }
}
