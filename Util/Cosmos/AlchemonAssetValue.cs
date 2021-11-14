using Newtonsoft.Json;

namespace Util.Cosmos
{
    public class AlchemonAssetValue : AssetValue
    {
        [JsonProperty("rarity")]
        public string Rarity { get; set; }
    }
}
