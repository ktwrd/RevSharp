using RevSharp.Core.Models;
using RevSharp.Skidbot.Helpers;
using RevSharp.Skidbot.Models.ContentDetection;

namespace RevSharp.Skidbot.Modules;

public partial class ContentDetectionModule
{
    private async Task Command_Request(CommandInfo info, Message message)
    {
        var embed = new SendableEmbed()
        {
            Title = "Content Detection - Request Access"
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
        if (data.IsBanned)
        {
            embed.Description = $"Server is banned\nReason: `{data.BanReason}`";
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
        var data = await controller.Get(server.Id) ??
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

        var notificationChannel = await Client.GetChannel("01H0QMAXMHXT5YF5RD61ZGK1SZ") as TextChannel;
        await notificationChannel.SendMessage(
            new SendableEmbed()
            {
                Description = string.Join(
                    "\n", new string[]
                    {
                        $"```", $"ServerId: {server.Id}", $"Name: {server.Name}", $"Members: {server.Members.Count}",
                        $"```"
                    })
            });  
        
        return true;
    }
}