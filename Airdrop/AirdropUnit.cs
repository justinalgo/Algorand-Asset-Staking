namespace Airdrop
{
    public class AirdropUnit
    {
        public string Address { get; set; }
        public ulong DropAssetId { get; set; }
        public ulong SourceAssetId { get; set; }
        public ulong NumberOfSourceAsset { get; set; }
        public double Value { get; set; }
        public bool IsMultiplied { get; }

        public AirdropUnit(string address, ulong dropAssetId, ulong sourceAssetId, double value, ulong numberOfSourceAsset = 1, bool isMultiplied = false)
        {
            this.Address = address;
            this.DropAssetId = dropAssetId;
            this.SourceAssetId = sourceAssetId;
            this.NumberOfSourceAsset = numberOfSourceAsset;
            this.Value = value;
            this.IsMultiplied = isMultiplied;
        }

        public ulong Amount()
        {
            if (this.IsMultiplied)
            {
                return (ulong)(this.NumberOfSourceAsset * this.Value);
            }
            else
            {
                return (ulong)this.Value;
            }
        }
    }
}
