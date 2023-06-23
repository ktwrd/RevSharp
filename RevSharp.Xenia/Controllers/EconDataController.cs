using MongoDB.Driver;
using RevSharp.Xenia.Models;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Modules;

[RevSharpModule]
public class EconDataController : BaseMongoController<EconProfileModel>
{
    public EconDataController()
        : base("econData")
    {}

    public async Task<EconProfileModel?> Get(string userId, string serverId)
    {
        var collection = GetCollection();
        var filter = Builders<EconProfileModel>
            .Filter
            .Where(v => v.UserId == userId && v.ServerId == serverId);
        var result = await collection.FindAsync(filter);
        return result.FirstOrDefault();
    }

    public async Task<IEnumerable<EconProfileModel>?> GetMany(string serverId)
    {
        var collection = GetCollection();
        var filter = Builders<EconProfileModel>
            .Filter
            .Where(v => v.ServerId == serverId);
        var result = await collection.FindAsync(filter);
        return result.ToEnumerable();
    }
    public async Task Set(EconProfileModel model)
    {
        var collection = GetCollection();
        var filter = Builders<EconProfileModel>
            .Filter
            .Where(v => v.UserId == model.UserId && v.ServerId == model.ServerId);
        var exists = (await collection.FindAsync(filter))?.Any() ?? false;
        if (exists)
            await collection.ReplaceOneAsync(filter, model);
        else
            await collection.InsertOneAsync(model);
    }
}