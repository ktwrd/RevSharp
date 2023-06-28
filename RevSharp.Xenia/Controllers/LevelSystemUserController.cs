using MongoDB.Driver;
using RevSharp.Core;
using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Models;
using RevSharp.Xenia.Modules;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Controllers;

[RevSharpModule]
public class LevelSystemUserController : BaseMongoController<LevelUserModel>
{
    public LevelSystemUserController()
        : base("levelSystemUser")
    {
        _random = new Random();
    }

    public async Task Migrate()
    {
        var targetCollection = GetCollection();
        if ((await targetCollection.FindAsync(Builders<LevelUserModel>.Filter.Empty))?.Any() ?? true)
        {
            Log.Warn("Already migrated");
            throw new Exception("Migrated already");
            return;
        }
        
        var sourceCollection = GetCollection<LevelMemberModel>("levelSystem");
        var dict = new Dictionary<string, LevelUserModel>();
        var sourceItems = await sourceCollection.FindAsync(Builders<LevelMemberModel>.Filter.Empty);
        foreach (var item in await sourceItems.ToListAsync())
        {
            if (!dict.ContainsKey(item.UserId))
                dict.TryAdd(
                    item.UserId, new LevelUserModel()
                    {
                        UserId = item.UserId
                    });

            dict[item.UserId].ServerPair.TryAdd(item.ServerId, item.Xp);
            if (dict[item.UserId].LastMessageTimestamp < item.LastMessageTimestamp)
                dict[item.UserId].LastMessageTimestamp = item.LastMessageTimestamp;
        }

        foreach (var item in dict)
        {
            try
            {
                await targetCollection.InsertOneAsync(item.Value);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to migrate item {item.Key}\n{ex}");
            }
        }
    }

    private readonly Random _random;
    public async Task<LevelUserModel?> Get(string userId)
    {
        var collection = GetCollection();
        var filter = Builders<LevelUserModel>
            .Filter
            .Where(v => v.UserId == userId);

        var result = await collection.FindAsync(filter);
        var item = result.FirstOrDefault();
        return item;
    }

    public async Task Set(LevelUserModel model)
    {
        var collection = GetCollection();
        var filter = Builders<LevelUserModel>
            .Filter
            .Where(v => v.UserId == model.UserId);

        var exists = (await collection.FindAsync(filter))?.Any() ?? false;
        if (exists)
            await collection.ReplaceOneAsync(filter, model);
        else
            await collection.InsertOneAsync(model);
    }
}