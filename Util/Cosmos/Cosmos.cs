using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Dynamic;

namespace Util.Cosmos
{
    public interface ICosmos
    {
        Task<AssetValue> CreateAsset(AssetValue assetValue);
        Task CreateAsset(dynamic assetValue, string projectId);
        public Task<AssetValue> GetAssetValueById(long assetId, string key);
        public Task<IEnumerable<AssetValue>> GetAssetValues(string projectId);
        public Task<IEnumerable<AssetValue>> GetAssetValues(string projectId, params string[] projectIds);
        Task<IEnumerable<AssetValue>> GetAssetValuesSql(string sql, string projectId = null);
    }

    public class Cosmos : ICosmos
    {
        private readonly CosmosClient client;
        private readonly Container assetsContainer;

        public Cosmos(IConfiguration config)
        {
            CosmosClientOptions options = new CosmosClientOptions { AllowBulkExecution = true };
            client = new CosmosClient(config["Endpoints:Cosmos"], config["CosmosPrimaryKey"], options);
            assetsContainer = client.GetContainer("caverna", "Assets");
        }

        public async Task<AssetValue> GetAssetValueById(long assetId, string key)
        {
            AssetValue assetValue = await assetsContainer.ReadItemAsync<AssetValue>(assetId.ToString(), new PartitionKey(key));
            
            return assetValue;
        }

        public async Task<AssetValue> CreateAsset(AssetValue assetValue)
        {
            AssetValue av = await assetsContainer.CreateItemAsync<AssetValue>(assetValue, new PartitionKey(assetValue.ProjectId));
            return av;
        }

        public async Task CreateAsset(dynamic assetValue, string projectId)
        {
            await assetsContainer.CreateItemAsync(assetValue, new PartitionKey(projectId));
        }

        public async Task<IEnumerable<AssetValue>> GetAssetValues(string projectId)
        {
            string sql = "SELECT * FROM c";
            QueryRequestOptions options = new QueryRequestOptions()
            {
                PartitionKey = new PartitionKey(projectId),
            };

            List<AssetValue> assetValues = new List<AssetValue>();

            FeedIterator<AssetValue> iterator = this.assetsContainer.GetItemQueryIterator<AssetValue>(sql, requestOptions: options);

            while (iterator.HasMoreResults)
            {
                FeedResponse<AssetValue> response = await iterator.ReadNextAsync();
                assetValues.AddRange(response);
            }

            return assetValues;
        }

        public async Task<IEnumerable<AssetValue>> GetAssetValues(string projectId, params string[] projectIds)
        {
            List<string> ids = new List<string>();
            ids.Add(projectId);
            ids.AddRange(projectIds);

            List<AssetValue> assetValues = new List<AssetValue>();

            FeedIterator<AssetValue> iterator = this.assetsContainer.GetItemLinqQueryable<AssetValue>()
                .Where(av => ids.Contains(av.ProjectId))
                .ToFeedIterator<AssetValue>();

            while (iterator.HasMoreResults)
            {
                FeedResponse<AssetValue> response = await iterator.ReadNextAsync();
                assetValues.AddRange(response);
            }

            return assetValues;
        }

        public async Task<IEnumerable<AssetValue>> GetAssetValuesSql(string sql, string projectId = null)
        {

            QueryRequestOptions options = new QueryRequestOptions();

            if (projectId != null)
            {
                options.PartitionKey = new PartitionKey(projectId);
            }

            List<AssetValue> assetValues = new List<AssetValue>();

            FeedIterator<AssetValue> iterator = this.assetsContainer.GetItemQueryIterator<AssetValue>(sql, requestOptions: options);

            while (iterator.HasMoreResults)
            {
                FeedResponse<AssetValue> response = await iterator.ReadNextAsync();
                assetValues.AddRange(response);
            }

            return assetValues;
        }
    }
}
