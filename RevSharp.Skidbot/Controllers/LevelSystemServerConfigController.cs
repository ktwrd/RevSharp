using RevSharp.Skidbot.Models;
using RevSharp.Skidbot.Modules;
using MongoDB.Driver;
using RevSharp.Skidbot.Reflection;

namespace RevSharp.Skidbot.Controllers;

[RevSharpModule]
public class LevelSystemServerConfigController : BaseMongoController<LevelSystemServerConfigModel>
{
    public LevelSystemServerConfigController()
        : base("levelSystemServerConfig")
    {}
    
    public async Task<LevelSystemServerConfigModel?> Get(string serverId)
    {
        var collection = GetCollection();
        var filter = MongoDB.Driver.Builders<LevelSystemServerConfigModel>
            .Filter
            .Where(v => v.ServerId == serverId);

        var result = await collection.FindAsync(filter);
        var item = result.FirstOrDefault();
        return item;
    }

    public async Task Set(LevelSystemServerConfigModel model)
    {
        var collection = GetCollection();
        var filter = MongoDB.Driver.Builders<LevelSystemServerConfigModel>
            .Filter
            .Where(v => v.ServerId == model.ServerId);

        var exists = (await collection.FindAsync(filter))?.Any() ?? false;
        if (exists)
            await collection.ReplaceOneAsync(filter, model);
        else
            await collection.InsertOneAsync(model);
    }
}