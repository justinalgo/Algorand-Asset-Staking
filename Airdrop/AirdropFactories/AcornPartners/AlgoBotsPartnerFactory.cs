using Algorand.V2.Indexer.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils.Algod;
using Utils.Indexer;

namespace Airdrop.AirdropFactories.AcornPartners
{
    public class AlgoBotsPartnerFactory : AcornPartner
    {
        public AlgoBotsPartnerFactory(IIndexerUtils indexerUtils, IAlgodUtils algodUtils, ulong totalWinnings) : base(indexerUtils, algodUtils)
        {
            this.CreatorAddresses = new string[] {
                "NZTRBMNPONCBXZX3MIQUKCUSN4ONAGKLQN445XC7NMQ2WR7HLVWYRTVRRY"};
            this.AssetPrefix = "Algobot";
            this.NumberOfWinners = 10;
            this.TotalWinnings = totalWinnings;
        }
    }
}
