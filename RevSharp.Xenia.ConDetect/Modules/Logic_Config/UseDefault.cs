using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Models.ContentDetection;

namespace RevSharp.Xenia.Modules;

public partial class ConDetectConfigModule
{
    public async Task Command_UseDefault(CommandInfo info, Message message)
    {
        await message.AddReaction(Client, "✅");
        var action = "";
        if (info.Arguments.Count > 1)
            action = info.Arguments[1].ToLower();

        if (action != "delete" && action != "flag")
        {
            await message.Reply($"Invalid action `{action}`. Must be `delete` or `flag`");
            return;
        }

        var embed = new SendableEmbed()
        {
            Title = "Content Detection Config - Default",
            Colour = CommandHelper.DefaultColor
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

        if (action == "delete")
        {
            data.DeleteThreshold = AnalysisServerConfig.DefaultDeleteThreshold;
        }
        else if (action == "flag")
        {
            data.FlagThreshold = AnalysisServerConfig.DefaultFlagThreshold;
        }


        embed.Description = action switch
        {
            "delete" => $"Reset delete threshold to defaults.",
            "flag" => $"Reset flag threshold to defaults."
        };
        try
        {
            await configController.Set(data);
        }
        catch (Exception ex)
        {
            embed.Description = $"Failed to save config. `{ex.Message}`";
            embed.Colour = CommandHelper.ErrorColor;
            await message.Reply(embed);
            await ReportError(ex, message, $"Failed to save config for server {server.Id}");
            return;
        }
        await message.Reply(embed);
    }
}