using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Models.ContentDetection;

namespace RevSharp.Xenia.Modules;

public partial class ContentDetectionModule
{
    private async Task Command_SetLogChannel(CommandInfo info, Message message)
    {
        await message.AddReaction(Client, "✅");
        var embed = new SendableEmbed()
        {
            Title = "Content Detection - Set Log Channel"
        };
        
        var server = await message.FetchServer();
        var member = await server.GetMember(message.AuthorId);
        if (!await member.HasPermission(PermissionFlag.ManageServer))
        {
            embed.Description = $"You do not have the required permission, `ManageServer`";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }

        var controller = Reflection.FetchModule<ContentDetectionServerConfigController>();
        var data = await controller.Get(server.Id) ??
                   new AnalysisServerConfig()
                   {
                       ServerId = server.Id
                   };
        data.ServerId = server.Id;
        data.LogChannelId = message.ChannelId;
        try
        {
            await controller.Set(data);
        }
        catch (Exception e)
        {
            embed.Description = $"Failed to set log channel\n```\n{e}\n```";
            embed.Colour = "red";
            await message.Reply(embed);
            Log.Error(e);
            return;
        }
        embed.Description = $"Set Log channel to <#{message.ChannelId}>";
        await message.Reply(embed);
    }
}