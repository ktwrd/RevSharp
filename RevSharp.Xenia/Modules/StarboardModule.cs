using RevSharp.Core.Models;
using RevSharp.Xenia.Controllers;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Models;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Modules;

public class StarboardModule : CommandModule
{

    public override Task InitComplete()
    {
        Client.MessageReactAdd += async (id, react, messageId, channelId) =>
        {
            if (react != "⭐")
                return;
            if (!(await Client.GetChannel(channelId) is TextChannel channel))
                return;

            var server = await Client.GetServer(channel.ServerId);
            var controller = Reflection.FetchModule<StarboardConfigController>();
            var data = await controller.Get(server.Id)
                       ?? new StarboardConfigModel()
                       {
                           ServerId = server.Id
                       };
            await controller.Set(data);
            if (data.ChannelId.Length < 1)
                return;
            if (data.ChannelId == channelId)
                return;
            if (data.ProxyMessageMap.ContainsKey(messageId))
                return;

            var message = await channel.GetMessage(messageId);

            data = await controller.Get(server.Id);
            data.MessageReact.TryAdd(messageId, 0);
            data.MessageReact[messageId]++;
            await controller.Set(data);
            if (data.MessageReact[messageId] >= data.MinimumRequired)
            {
                await channel.SendMessage(
                    new DataMessageSend()
                    {
                        Embeds = new SendableEmbed[]
                        {
                            new SendableEmbed()
                            {
                                Description = string.Join(
                                    "\n",
                                    new string[]
                                    {
                                        $"Author <@{message.AuthorId}>",
                                        message.Content ?? "<empty>",
                                    }),
                                Media = message?.Attachments?.Select(v => v.Id).FirstOrDefault(),
                                Url = $"https://app.revolt.chat/servers/{server.Id}/channels/{channel.Id}/{messageId}"
                            }
                        }
                    });
            }
        };
        return Task.CompletedTask;
    }
    public override async Task CommandReceived(CommandInfo info, Message message)
    {
        var action = "";
        if (info.Arguments.Count > 0)
            action = info.Arguments[0];

        switch (action)
        {
            case "help":
                await Command_Help(info, message);
                break;
            case "setchannel":
                await Command_SetChannel(info, message);
                break;
            default:
                await message.Reply($"Unknown Action {action,-1}");
                break;
        }
    }

    public async Task Command_Help(CommandInfo info, Message message)
    {
        await message.Reply(
            new SendableEmbed()
            {
                Title = "Starboard - Help",
                Description = HelpContent()
            });
    }
    public async Task Command_SetChannel(CommandInfo info, Message message)
    {
        var embed = new SendableEmbed()
        {
            Title = "Starboard - Set Channel"
        };
        var server = await message.FetchServer();
        var controller = Reflection.FetchModule<StarboardConfigController>();
        var data = await controller.Get(server.Id)
                   ?? new StarboardConfigModel()
                   {
                       ServerId = server.Id
                   };
        try
        {
            await controller.Set(data);
            data.ChannelId = message.ChannelId;
            await controller.Set(data);
        }
        catch (Exception ex)
        {
            embed.Colour = "red";
            embed.Description = string.Join(
                "\n", new string[]
                {
                    "Failed to save to database", "```", ex.Message, ex.StackTrace, "```"
                });
            await message.Reply(embed);
            return;
        }

        embed.Colour = CommandHelper.DefaultColor;
        embed.Description = $"Set starboard channel to <#{message.ChannelId}>";
        await message.Reply(embed);
    }

    public override string? HelpContent()
    {
        return XeniaHelper.GenerateHelp(this, new List<(string, string)>()
        {
            ("setchannel", "Set channel to proxy starboard messages to"),
            ("help", "Display this message")
        });
    }
    public override string? HelpCategory => null;
    public override bool HasHelpContent => true;
    public override string? BaseCommandName => "starboard";
    public override PermissionFlag? RequirePermission => PermissionFlag.ManageServer;
    public override bool ServerOnly => true;
}