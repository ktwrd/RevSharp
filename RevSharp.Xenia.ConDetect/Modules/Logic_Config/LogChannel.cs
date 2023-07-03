using RevSharp.Core;
using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Models.ContentDetection;

namespace RevSharp.Xenia.Modules;

public partial class ConDetectConfigModule
{
    public async Task Command_LogChannel(CommandInfo info, Message message)
    {
        await message.AddReaction(Client, "✅");
        var embed = new SendableEmbed()
        {
            Title = "Content Detection Config - Log Channel",
        };
        
        var server = await message.FetchServer();
        var configController = Reflection.FetchModule<ContentDetectionServerConfigController>();
        if (configController == null)
        {
            embed.Colour = CommandHelper.ErrorColor;
            embed.Description = $"Failed to get config controller (is null)";
            await message.Reply(embed);
            await ReportError(new Exception($"ContentDetectionServerConfigController is null (server: {server.Id}"), message);
            return;
        }
        var data = await configController.Get(server.Id) ??
               new AnalysisServerConfig()
               {
                   ServerId = server.Id
               };

        string? targetChannelId = message.ChannelId;
        if (info.Arguments.Count > 1)
        {
            if (info.Arguments[1] != "this")
                targetChannelId = CommandHelper.FindChannelId(info.Arguments[1]);
        }

        if (targetChannelId == null)
        {
            embed.Description = $"Channel provided is invalid";
            embed.Colour = CommandHelper.ErrorColor;
            await message.Reply(embed);
            return;
        }

        try
        {
            await Client.GetChannel(targetChannelId);
        }
        catch (RevoltException rex)
        {
            embed.Description = $"Failed to verify channel. `{rex.Message}`";
            embed.Colour = CommandHelper.ErrorColor;
            await message.Reply(embed);
            return;
        }
        catch (Exception ex)
        {
            embed.Description = $"Failed to get channel! `{ex.Message}`";
            embed.Colour = CommandHelper.ErrorColor;
            await message.Reply(embed);
            await ReportError(ex, message, "Failed to get channel while validating");
            return;
        }

        string targetActionId = "any";
        if (info.Arguments.Count > 2)
            targetActionId = info.Arguments[2];
        if (new string[]
            {
                "any", "delete", "flag"
            }.Contains(targetActionId) == false)
        {
            embed.Description = $"Invalid action `{targetActionId,-1}`";
            embed.Colour = CommandHelper.ErrorColor;
            await message.Reply(embed);
            return;
        }

        string targetTypeId = "any";
        if (info.Arguments.Count > 3)
            targetTypeId = info.Arguments[3];
        if (new string[]
            {
                "any", "text", "media"
            }.Contains(targetTypeId) ==
            false)
        {
            embed.Description = $"Invalid type `{targetTypeId,-1}`";
            embed.Colour = CommandHelper.ErrorColor;
            await message.Reply(embed);
            return;
        }

        if (targetTypeId == "text")
        {
            if (targetActionId == "any")
            {
                data.LogChannelId_TextDelete = targetChannelId;
                data.LogChannelId_TextFlag = targetChannelId;
            }
            else if (targetActionId == "delete")
            {
                data.LogChannelId_TextDelete = targetChannelId;
            }
            else if (targetActionId == "flag")
            {
                data.LogChannelId_TextFlag = targetChannelId;
            }
        }
        else if (targetTypeId == "media")
        {
            if (targetActionId == "any")
            {
                data.LogChannelId_MediaDelete = targetChannelId;
                data.LogChannelId_MediaFlag = targetChannelId;
            }
            else if (targetActionId == "delete")
            {
                data.LogChannelId_MediaDelete = targetChannelId;
            }
            else if (targetActionId == "flag")
            {
                data.LogChannelId_MediaFlag = targetChannelId;
            }
        }
        else if (targetTypeId == "any")
        {
            data.LogChannelId = targetChannelId;
            data.LogChannelId_TextDelete = targetChannelId;
            data.LogChannelId_TextFlag = targetChannelId;
            data.LogChannelId_MediaDelete = targetChannelId;
            data.LogChannelId_MediaFlag = targetChannelId;
        }
        try
        {
            await configController.Set(data);
        }
        catch (Exception ex)
        {
            embed.Description = $"Failed to save! `{ex.Message}`";
            embed.Colour = CommandHelper.ErrorColor;
            await message.Reply(embed);
            await ReportError(ex, message, "Failed to save config");
            return;
        }

        embed.Description = $"Set log channel to <#{targetChannelId}> ({targetActionId}, {targetTypeId})";
        embed.Colour = CommandHelper.DefaultColor;
        await message.Reply(embed);
    }
    
}