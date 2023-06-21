using MongoDB.Driver;
using RevSharp.Skidbot.Models.ContentDetection;
using RevSharp.Skidbot.Reflection;

namespace RevSharp.Skidbot.Modules;

[RevSharpModule]
public class ContentDetectionServerConfigController : BaseMongoController<AnalysisServerConfig>
{
    public ContentDetectionServerConfigController()
        : base("contentDetectionServerConfig")
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
            ConfigCache.TryAdd(item.ServerId, item);
            ConfigCache[item.ServerId] = item;
            
            var s = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            ConfigCacheLastSet.TryAdd(item.ServerId, s);
            ConfigCacheLastSet[item.ServerId] = s;
        }
        return item;
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
        if (ConfigCacheLastSet.TryGetValue(serverId, out var lastUpdated))
            if ((DateTimeOffset.UtcNow.ToUnixTimeSeconds() - lastUpdated) > 120)
                return await Fetch(serverId);
        if (ConfigCache.TryGetValue(serverId, out var value))
            return value;

        return await Fetch(serverId);
    }
    
    public async Task Set(AnalysisServerConfig model)
    {
        var collection = GetCollection();
        var filter = Builders<AnalysisServerConfig>
            .Filter
            .Where(v => v.ServerId == model.ServerId);

        var exists = (await collection.FindAsync(filter))?.Any() ?? false;
        if (exists)
            await collection.ReplaceOneAsync(filter, model);
        else
            await collection.InsertOneAsync(model);
        
        ConfigCache.TryAdd(model.ServerId, model);
        ConfigCache[model.ServerId] = model;
    }
}