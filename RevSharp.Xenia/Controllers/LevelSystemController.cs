using MongoDB.Driver;
using RevSharp.Core;
using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Models;
using RevSharp.Xenia.Modules;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Controllers;

[RevSharpModule]
public class LevelSystemController : BaseModule
{
    public LevelSystemController()
    {
        _random = new Random();
    }

    private readonly Random _random;

    public override async Task MessageReceived(Message message)
    {
        if (message.IsSystemMessage || message.Masquerade != null)
            return;
        if (message.AuthorId == Client.CurrentUserId)
            return;

        User? author = null;
        try
        {
            author = await Client.GetUser(message.AuthorId);
        }
        catch (Exception ex)
        {
            await ReportError(ex, null, $"Failed to fetch user {message.AuthorId}");
        }
        if (author == null || author.IsBot)
            return;

        var server = await message.FetchServer();
        if (server == null)
            return;

        var dataController = Reflection.FetchModule<LevelSystemUserController>();
        var data = await dataController.Get(message.AuthorId);
        if (data == null)
            data = new LevelUserModel()
            {
                UserId = message.AuthorId
            };
        await dataController.Set(data);
        
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
                if (!serverData.Enable)
                    return;
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
                {
                    try
                    {
                        await message.Reply(resultEmbed);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            await message.Reply(resultEmbed.Description);
                        }
                        catch (Exception exx){}
                    }
                }
            }
        }
    }
    
    public async Task<(bool, ExperienceMetadata)> GrantXp(LevelUserModel model, Message message, int? a = null)
    {
        var server = await message.FetchServer();
        var dataController = Reflection.FetchModule<LevelSystemUserController>();
        var data = await dataController.Get(model.UserId);
        if (data == null)
            data = new LevelUserModel()
            {
                UserId = model.UserId
            };
        data.ServerPair.TryAdd(server.Id, 0);
        await dataController.Set(data);
        var amount = a ?? (int)_random.Next(1, 5);

        // Generate previous and current metadata
        var metadataPrevious = XpHelper.Generate(data, server.Id);
        if (a != null)
            data.ServerPair[server.Id] = (ulong)a;
        else
            data.ServerPair[server.Id] += (ulong)amount;
        var metadata = XpHelper.Generate(data, server.Id);

        // Set previous Ids
        data.LastMessageChannelId = message.ChannelId;
        data.LastMessageId = message.Id;
        data.LastMessageTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        await dataController.Set(data);

        bool levelUp = metadataPrevious.UserLevel != metadata.UserLevel;
        if (levelUp)
        {
            OnUserLevelUp(model, metadataPrevious, metadata);
        }

        await dataController.Set(data);
        return (levelUp, metadata);
    }
    protected void OnUserLevelUp(LevelUserModel model, ExperienceMetadata previous, ExperienceMetadata current)
    {
        if (UserLevelUp != null)
        {
            UserLevelUp?.Invoke(model, previous, current);
        }
    }
    public event ExperienceComparisonDelegate UserLevelUp;
}