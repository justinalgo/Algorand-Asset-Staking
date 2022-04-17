using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class AlgoGangPartnerFactory : AcornPartner
    {
        public AlgoGangPartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] {
                "GANGAAWKBJBWQJIIETTLWQT7ZFGPC4UDIITNGP55BCQPB26IEMOPOHQMEA",
                "GANGEG3ZYV7YJ7XJK5UBI6QUOCJKTZ3MVF7I5NYF7LINYMGQGZURKKPIPI"};
            this.RevokedAssets = new ulong[]
            {
                310671522, 334052535, 334085231, 334093221, 350690308, 361182724, 385653819, 385655333, 385658067
            };
            this.NumberOfWinners = 10;
            this.TotalWinnings = totalWinnings;
        }
    }
}
