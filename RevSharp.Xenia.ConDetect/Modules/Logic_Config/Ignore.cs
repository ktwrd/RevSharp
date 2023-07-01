using RevSharp.Core;
using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Models.ContentDetection;

namespace RevSharp.Xenia.Modules;

public partial class ConDetectConfigModule
{
    private async Task Command_Ignore(CommandInfo info, Message message)
    {
        await message.AddReaction(Client, "✅");
        var embed = new SendableEmbed()
        {
            Title = "Content Detection Config - Ignore",
        };
        
        var server = await message.FetchServer();
        var configController = Reflection.FetchModule<ContentDetectionServerConfigController>();
        if (configController == null)
        {
            embed.Colour = CommandHelper.ErrorColor;
            embed.Description = $"Failed to get config controller (is null)";
            await message.Reply(embed);
            await ReportError(new Exception($"ContentDetectionServerConfigController is null (server: {server?.Id}"), message);
            return;
        }
        var data = await configController.Get(server.Id) ??
           new AnalysisServerConfig()
           {
               ServerId = server.Id
           };

        string targetType = "";
        if (info.Arguments.Count > 1)
            targetType = info.Arguments[1];
        if (targetType != "user" && targetType != "channel")
        {
            embed.Description = $"Invalid type `{targetType,-1}`";
            embed.Colour = CommandHelper.ErrorColor;
            await message.Reply(embed);
            return;
        }

        string? targetId = null;
        if (info.Arguments.Count > 2)
            targetId = info.Arguments[2];
        if (targetType == "channel")
        {
            targetId = CommandHelper.FindChannelId(targetId ?? "");
        }
        else if (targetType == "user")
        {
            targetId = CommandHelper.FindUserId(targetId ?? "");
        }

        if (targetId == null)
        {
            embed.Description = $"Invalid Id `{targetId}`";
            embed.Colour = CommandHelper.ErrorColor;
            await message.Reply(embed);
            return;
        }

        if (targetType == "channel")
        {
            try
            {
                await Client.GetChannel(targetId);
            }
            catch (RevoltException rex)
            {
                embed.Description = $"Failed to get channel. `{rex.Message}`";
                embed.Colour = CommandHelper.ErrorColor;
                await message.Reply(embed);
                return;
            }
            catch (Exception ex)
            {
                embed.Description = $"Failed to get channel. `{ex.Message}`";
                embed.Colour = CommandHelper.ErrorColor;
                await message.Reply(embed);
                await ReportError(ex, message, $"Failed to fetch channel {targetId} in server {server.Id}");
                return;
            }

            data.IgnoredChannelIds = data.IgnoredChannelIds.Concat(
                new string[]
                {
                    targetId.ToUpper()
                }).ToArray();
        }
        else if (targetType == "user")
        {
            data.IgnoredAuthorIds = data.IgnoredAuthorIds.Concat(
                new string[]
                {
                    targetId.ToUpper()
                }).ToArray();
        }

        try
        {
            await configController.Set(data);
        }
        catch (Exception ex)
        {
            embed.Description = $"Failed to save data! {ex.Message}";
            embed.Colour = CommandHelper.ErrorColor;
            await message.Reply(embed);
            await ReportError(ex, message, $"Failed to set config data for server {server.Id}");
            return;
        }

        embed.Description = "Success!";
        embed.Colour = CommandHelper.DefaultColor;
        await message.Reply(embed);
    }
}