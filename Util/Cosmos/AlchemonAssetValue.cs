using Newtonsoft.Json;

namespace Util.Cosmos
{
    public class AlchemonAssetValue : AssetValue
    {
        [JsonProperty("rarity")]
        public string Rarity { get; set; }
        [JsonProperty("set")]
        public int Set { get; set; }
    }
}
