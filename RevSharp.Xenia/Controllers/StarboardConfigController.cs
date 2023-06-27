using MongoDB.Driver;
using RevSharp.Xenia.Models;
using RevSharp.Xenia.Modules;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Controllers;

[RevSharpModule]
public class StarboardConfigController : BaseMongoController<StarboardConfigModel>
{
    public StarboardConfigController()
        : base("starboard")
    { }

    public async Task<StarboardConfigModel?> Get(string serverId)
    {
        var filter = Builders<StarboardConfigModel>
            .Filter
            .Where(v => v.ServerId == serverId);
        var col = GetCollection();
        var res = await col.FindAsync(filter);
        return res.FirstOrDefault();
    }

    public async Task Set(StarboardConfigModel model)
    {
        var filter = Builders<StarboardConfigModel>
            .Filter
            .Where(v => v.ServerId == model.ServerId);
        var col = GetCollection();
        var exists = (await col.FindAsync(filter))?.Any() ?? false;
        if (exists)
            await col.ReplaceOneAsync(filter, model);
        else
            await col.InsertOneAsync(model);
    }
}