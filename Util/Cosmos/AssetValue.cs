using Newtonsoft.Json;

namespace Util
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
        public long AssetId { get; set; }
        [JsonProperty("value")]
        public long Value { get; set; }
    }
}
