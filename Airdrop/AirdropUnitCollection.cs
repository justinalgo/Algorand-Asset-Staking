using System.Collections.Concurrent;

namespace Airdrop
{
    public class AirdropUnitCollection
    {
        public string Wallet { get; }
        public ulong DropAssetId { get; }
        public ulong Total { get => this.GetTotal(); }
        public ConcurrentBag<AirdropUnit> airdropUnits;

        public AirdropUnitCollection(string wallet, ulong dropAssetId)
        {
            this.Wallet = wallet;
            this.DropAssetId = dropAssetId;
            this.airdropUnits = new ConcurrentBag<AirdropUnit>();
        }

        public AirdropUnitCollection((string, ulong) infoTuple) : this(infoTuple.Item1, infoTuple.Item2) { }

        public AirdropUnitCollection(string wallet, ulong dropAssetId, AirdropUnit airdropUnit) : this(wallet, dropAssetId)
        {
            this.AddAirdropUnit(airdropUnit);
        }

        public AirdropUnitCollection((string, ulong) infoTuple, AirdropUnit airdropUnit) : this(infoTuple.Item1, infoTuple.Item2, airdropUnit) { }

        public void AddAirdropUnit(AirdropUnit airdropUnit)
        {
            this.airdropUnits.Add(airdropUnit);
        }

        public ulong GetTotal()
        {
            ulong total = 0;

            foreach (AirdropUnit airdropUnit in this.airdropUnits)
            {
                total += airdropUnit.Amount();
            }

            return total;
        }

        public int Count()
        {
            return this.airdropUnits.Count;
        }
    }
}
