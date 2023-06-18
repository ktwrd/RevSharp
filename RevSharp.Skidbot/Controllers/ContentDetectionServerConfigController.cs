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
        }
        return item;
    }

    private Dictionary<string, AnalysisServerConfig> ConfigCache = new Dictionary<string, AnalysisServerConfig>();

    public async Task<AnalysisServerConfig?> Get(string serverId)
    {
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