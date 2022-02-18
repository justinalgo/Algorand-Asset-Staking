using Newtonsoft.Json;

namespace Utils.Cosmos
{
    public class AssetValue
    {
        public AssetValue() { }

        public AssetValue(ulong assetId, string projectId, string name, string unitName, ulong value)
        {
            this.Id = assetId.ToString();
            this.ProjectId = projectId;
            this.Name = name;
            this.UnitName = unitName;
            this.AssetId = assetId;
            this.Value = value;
        }

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
