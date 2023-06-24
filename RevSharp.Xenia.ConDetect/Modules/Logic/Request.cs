using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Models.ContentDetection;

namespace RevSharp.Xenia.Modules;

public partial class ContentDetectionModule
{
    private async Task Command_Request(CommandInfo info, Message message)
    {
        var embed = new SendableEmbed()
        {
            Title = "Content Detection - Request Access"
        };
        
        
        var server = await message.FetchServer();
        var member = await server.GetMember(message.AuthorId, false);
        if (!await member.HasPermission(PermissionFlag.ManageServer))
        {
            embed.Description = $"You do not have the required permission, `ManageServer`";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }
        
        var controller = Reflection.FetchModule<ContentDetectionServerConfigController>();
        var data = await controller.Fetch(server.Id) ??
                   new AnalysisServerConfig()
                   {
                       ServerId = server.Id
                   };
        if (data.IsBanned)
        {
            embed.Description = $"Server is banned\nReason: `{data.BanReason}`";
            embed.Colour = "red";
        }
        else if (data.LogChannelId == null || data.LogChannelId.Length < 1)
        {
            embed.Description =
                $"Log Channel is not set. Please use `{Reflection.Config.Prefix}{BaseCommandName} setlogchannel` in the channel that you want logs to go to.";
            embed.Colour = "red";
        }
        else if (data.HasRequested)
        {
            embed.Description = data.AllowAnalysis switch
            {
                true => "You already have access",
                false => "Your server is still pending approval. Please wait",
            };
            embed.Colour = "yellow";
        }
        else if (data.AllowAnalysis)
        {
            embed.Description = "You already have access";
            embed.Colour = "yellow";
        }
        else
        {
            bool success = await RequestAccess(server.Id);
            if (success)
            {
                embed.Description = "Request sent. Please wait for a moderator to approve your server";
                embed.Colour = "green";
            }
            else
            {
                embed.Description = "Failed to request access";
                embed.Colour = "red";
            }
        }

        await message.Reply(embed);
    }
    
    private async Task<bool> RequestAccess(string serverId)
    {
        var server = await Client.GetServer(serverId);
        var controller = Reflection.FetchModule<ContentDetectionServerConfigController>();
        var data = await controller.Fetch(server.Id) ??
                   new AnalysisServerConfig()
                   {
                       ServerId = server.Id
                   };
        if (data.IsBanned)
            return false;
        if (data.AllowAnalysis)
            return false;

        data.HasRequested = true;
        data.AllowAnalysis = false;

        var notificationChannel = await Client.GetChannel(Reflection.Config.LogChannelId) as TextChannel;
        await notificationChannel.SendMessage(
            new SendableEmbed()
            {
                Description = string.Join(
                    "\n",
                    new string[]
                    {
                        $"```",
                        $"ServerId: {server.Id}",
                        $"Name: {server.Name}",
                        $"Members: {server.Members.Count}",
                        $"```"
                    })
            });
        await controller.Set(data);
        
        return true;
    }
}