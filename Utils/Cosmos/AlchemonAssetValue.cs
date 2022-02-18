using Newtonsoft.Json;

namespace Utils.Cosmos
{
    public class AlchemonAssetValue : AssetValue
    {
        public AlchemonAssetValue() : base() { }

        public AlchemonAssetValue(ulong assetId, string projectId, string name, string unitName, ulong value, string rarity, int set) : base(assetId, projectId, name, unitName, value)
        {
            this.Rarity = rarity;
            this.Set = set;
        }

        [JsonProperty("rarity")]
        public string Rarity { get; set; }
        [JsonProperty("set")]
        public int Set { get; set; }
    }
}
