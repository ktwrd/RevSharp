using RevSharp.Xenia.Models;
using RevSharp.Xenia.Modules;
using MongoDB.Driver;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Controllers;

[RevSharpModule]
public class LevelSystemServerConfigController : BaseMongoController<LevelSystemServerConfigModel>
{
    public LevelSystemServerConfigController()
        : base("levelSystemServerConfig")
    {}
    
    public async Task<LevelSystemServerConfigModel?> Get(string serverId)
    {
        var collection = GetCollection();
        var filter = Builders<LevelSystemServerConfigModel>
            .Filter
            .Where(v => v.ServerId == serverId);

        var result = await collection.FindAsync(filter);
        var item = result.FirstOrDefault();
        return item;
    }

    public async Task Set(LevelSystemServerConfigModel model)
    {
        var collection = GetCollection();
        var filter = Builders<LevelSystemServerConfigModel>
            .Filter
            .Where(v => v.ServerId == model.ServerId);

        var exists = (await collection.FindAsync(filter))?.Any() ?? false;
        if (exists)
            await collection.ReplaceOneAsync(filter, model);
        else
            await collection.InsertOneAsync(model);
    }
}