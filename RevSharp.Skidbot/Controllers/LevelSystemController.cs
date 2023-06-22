using MongoDB.Driver;
using RevSharp.Core;
using RevSharp.Core.Models;
using RevSharp.Skidbot.Helpers;
using RevSharp.Skidbot.Models;
using RevSharp.Skidbot.Modules;
using RevSharp.Skidbot.Reflection;

namespace RevSharp.Skidbot.Controllers;

[RevSharpModule]
public class LevelSystemController : BaseMongoController<LevelMemberModel>
{
    public LevelSystemController()
        : base("levelSystem")
    {
        _random = new Random();
    }

    private readonly Random _random;

    public override async Task MessageReceived(Message message)
    {
        if (message.IsSystemMessage || message.Masquerade != null)
            return;

        var server = await message.FetchServer();
        if (server == null)
            return;
        var data = await Get(message.AuthorId, server.Id);
        if (data == null)
            data = new LevelMemberModel()
            {
                UserId = message.AuthorId,
                ServerId = server.Id
            };
        await Set(data);
        
        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var previousMessageDiff = currentTimestamp - data.LastMessageTimestamp;
        if (previousMessageDiff >= 8000)
        {
            var (levelUp, metadata) = await GrantXp(data, message);
            if (levelUp)
            {
                var serverConfController = Reflection.FetchModule<LevelSystemServerConfigController>();
                var serverData = await serverConfController.Get(server.Id);
                if (serverData == null)
                    serverData = new LevelSystemServerConfigModel()
                    {
                        ServerId = server.Id
                    };
                await serverConfController.Set(serverData);
                var resultEmbed = new SendableEmbed()
                {
                    Description = $"You've advanced to *level {metadata.UserLevel}*!"
                };
                TextChannel? targetChannel = null;
                if (serverData.LogChannelId != null)
                {
                    try
                    {
                        targetChannel = await Client.GetChannel(serverData.LogChannelId, true) as TextChannel;
                    }
                    catch (RevoltException ex)
                    {
                        resultEmbed.Description += $"\n- Cannot access level-up channel (`{ex.Message}`)";
                    }
                    catch (Exception ex)
                    {
                        resultEmbed.Description += $"\n- Failed to access level-up channel (`{ex.Message}`)";
                    }
                }

                if (targetChannel != null)
                {
                    try
                    {
                        await targetChannel.SendMessage(new DataMessageSend()
                        {
                            Embeds = new []
                            {
                                resultEmbed
                            },
                            Content = $"<@{message.AuthorId}>"
                        });
                        return;
                    }
                    catch (Exception ex)
                    {
                        resultEmbed.Description += $"\n- Failed to send in level-up channel (`{ex.Message}`)";
                    }   
                }
                if (targetChannel == null)
                    await message.Reply(resultEmbed);
            }
        }
    }
    
    public async Task<(bool, ExperienceMetadata)> GrantXp(LevelMemberModel model, Message message, int? a = null)
    {
        var data = await Get(model.UserId, model.ServerId);
        if (data == null)
            data = new LevelMemberModel()
            {
                ServerId = model.ServerId,
                UserId = model.UserId
            };
        await Set(data);
        var amount = a ?? (int)_random.Next(1, 5);

        // Generate previous and current metadata
        var metadataPrevious = XpHelper.Generate(data);
        if (a != null)
            data.Xp = (ulong)a;
        else
            data.Xp += (ulong)amount;
        var metadata = XpHelper.Generate(data);

        // Set previous Ids
        data.LastMessageChannelId = message.ChannelId;
        data.LastMessageId = message.Id;

        bool levelUp = metadataPrevious.UserLevel != metadata.UserLevel;
        if (levelUp)
        {
            OnUserLevelUp(model, metadataPrevious, metadata);
        }

        await Set(data);
        return (levelUp, metadata);
    }
    protected void OnUserLevelUp(LevelMemberModel model, ExperienceMetadata previous, ExperienceMetadata current)
    {
        if (UserLevelUp != null)
        {
            UserLevelUp?.Invoke(model, previous, current);
        }
    }
    public event ExperienceComparisonDelegate UserLevelUp;

    public async Task<LevelMemberModel?> Get(string userId, string serverId)
    {
        var collection = GetCollection();
        var filter = Builders<LevelMemberModel>
            .Filter
            .Where(v => v.ServerId == serverId && v.UserId == userId);

        var result = await collection.FindAsync(filter);
        var item = result.FirstOrDefault();
        return item;
    }

    public async Task Set(LevelMemberModel model)
    {
        var collection = GetCollection();
        var filter = Builders<LevelMemberModel>
            .Filter
            .Where(v => v.ServerId == model.ServerId && v.UserId == model.UserId);

        var exists = (await collection.FindAsync(filter))?.Any() ?? false;
        if (exists)
            await collection.ReplaceOneAsync(filter, model);
        else
            await collection.InsertOneAsync(model);
    }
}