﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Util.Cosmos
{
    public interface ICosmos
    {
        Task<AssetValue> CreateAsset(AssetValue assetValue);
        Task CreateAsset(dynamic assetValue, string projectId);
        public Task<AssetValue> GetAssetValueById(long assetId, string key);
        public Task<IEnumerable<AssetValue>> GetAssetValues(string projectId);
        public Task<IEnumerable<AssetValue>> GetAssetValues(string projectId, params string[] projectIds);
        Task<IEnumerable<dynamic>> GetAssetValuesDynamic(string projectId);
        Task<IEnumerable<AssetValue>> GetAssetValuesSql(string sql, string projectId = null);
        Task<AssetValue> ReplaceAsset(AssetValue assetValue);
        Task ReplaceAsset(dynamic assetValue, string id, string projectId);
    }
}