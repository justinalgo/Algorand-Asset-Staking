namespace Airdrop
{
    public class RetrievedAsset
    {
        public string Name { get; set; }
        public string UnitName { get; set; }
        public long AssetId { get; set; }

        public RetrievedAsset(string Name, string UnitName, long AssetId)
        {
            this.Name = Name;
            this.UnitName = UnitName;
            this.AssetId = AssetId;
        }
    }
}
