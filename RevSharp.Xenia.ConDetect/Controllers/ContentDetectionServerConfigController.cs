using MongoDB.Driver;
using RevSharp.Xenia.Models.ContentDetection;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Modules;

[RevSharpModule]
public class ContentDetectionServerConfigController : BaseMongoController<AnalysisServerConfig>
{
    public ContentDetectionServerConfigController()
        : base(AnalysisServerConfig.CollectionName)
    {}

    public async Task<AnalysisServerConfig?> Fetch(string serverId)
    {
        var collection = GetCollection();
        var filter = Builders<AnalysisServerConfig>
            .Filter
            .Where(v => v.ServerId == serverId);

        var result = await collection.FindAsync(filter);
        var item = result.FirstOrDefault();
        if (item != null)
        {
            if (item.Guid == "00000000-0000-0000-0000-000000000000")
            {
                item.Guid = Guid.NewGuid().ToString();
            }
            ConfigCache.TryAdd(item.ServerId, item);
            ConfigCache[item.ServerId] = item;
            
            var s = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            ConfigCacheLastSet.TryAdd(item.ServerId, s);
            ConfigCacheLastSet[item.ServerId] = s;
        }
        return item;
    }

    public async Task<AnalysisServerConfig[]?> FetchAll()
    {
        var collection = GetCollection();
        var filter = Builders<AnalysisServerConfig>
            .Filter
            .Empty;

        var result = await collection.FindAsync(filter);
        return result.ToList().ToArray();
    }

    public Dictionary<string, long> ConfigCacheLastSet = new Dictionary<string, long>();

    private Dictionary<string, AnalysisServerConfig> ConfigCache = new Dictionary<string, AnalysisServerConfig>();

    /// <summary>
    /// If config was last fetched more than 1min ago, <see cref="Fetch"/> is called instead
    /// </summary>
    /// <param name="serverId"></param>
    /// <returns></returns>
    public async Task<AnalysisServerConfig?> Get(string serverId)
    {
        return await Fetch(serverId);
        if (ConfigCacheLastSet.TryGetValue(serverId, out var lastUpdated))
            if ((DateTimeOffset.UtcNow.ToUnixTimeSeconds() - lastUpdated) > 120)
                return await Fetch(serverId);

        if (ConfigCache.TryGetValue(serverId, out var value))
        {
            if (value.Guid == "00000000-0000-0000-0000-000000000000")
            {
                ConfigCache[serverId].Guid = Guid.NewGuid().ToString();
            }
            return value;
        }

    }
    
    public async Task Set(AnalysisServerConfig model)
    {
        var collection = GetCollection();
        var filter = Builders<AnalysisServerConfig>
            .Filter
            .Where(v => v.ServerId == model.ServerId);

        if (model.Guid == "00000000-0000-0000-0000-000000000000")
        {
            model.Guid = Guid.NewGuid().ToString();
        }
        var exists = (await collection.FindAsync(filter))?.Any() ?? false;
        if (exists)
            await collection.ReplaceOneAsync(filter, model);
        else
            await collection.InsertOneAsync(model);
        
        ConfigCache.TryAdd(model.ServerId, model);
        ConfigCache[model.ServerId] = model;
    }
}