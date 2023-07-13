using MongoDB.Driver;
using RevSharp.Xenia.Moderation.Models;
using RevSharp.Xenia.Modules;

namespace RevSharp.Xenia.Moderation.Controllers;

public class ServerLogConfigController : BaseMongoController<ServerLogConfigModel>
{
    public ServerLogConfigController()
        : base(ServerLogConfigModel.CollectionName)
    {}

    public async Task<ServerLogConfigModel?> Get(string serverId)
    {
        var collection = GetCollection();
        var filter = Builders<ServerLogConfigModel>
            .Filter
            .Where(v => v.ServerId == serverId);

        var result = await collection.FindAsync(filter);
        return result.FirstOrDefault();
    }

    public async Task Set(ServerLogConfigModel model)
    {
        var collection = GetCollection();
        var filter = Builders<ServerLogConfigModel>
            .Filter
            .Where(v => v.ServerId == model.ServerId);
        var exists = (await collection.FindAsync(filter))?.Any() ?? false;
        if (exists)
            await collection.ReplaceOneAsync(filter, model);
        else
            await collection.InsertOneAsync(model);
    }
}