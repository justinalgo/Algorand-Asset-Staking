using Newtonsoft.Json;

namespace Util.Cosmos
{
    public class AssetValue
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("projectId")]
        public string ProjectId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("unitName")]
        public string UnitName { get; set; }
        [JsonProperty("assetId")]
        public ulong AssetId { get; set; }
        [JsonProperty("value")]
        public ulong Value { get; set; }
    }
}
