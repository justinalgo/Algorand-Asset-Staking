using Newtonsoft.Json;

namespace Util
{
    public class AlchemonAssetValue : AssetValue
    {
        [JsonProperty("rarity")]
        public string Rarity { get; set; }
    }
}
