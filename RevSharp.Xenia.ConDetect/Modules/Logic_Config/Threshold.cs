using System.Text.Json;
using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Models.ContentDetection;

namespace RevSharp.Xenia.Modules;

public partial class ConDetectConfigModule
{
    public async Task Command_Threshold(CommandInfo info, Message message)
    {
        var embed = new SendableEmbed()
        {
            Title = "Content Detection Config - Threshold"
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

        string baseAction = "";
        if (info.Arguments.Count > 1)
            baseAction = info.Arguments[1];
        if (baseAction != "get" && baseAction != "set")
        {
            await message.Reply($"Invalid action `{baseAction}`");
            return;
        }


        string targetAction = "";
        if (info.Arguments.Count > 2)
            targetAction = info.Arguments[2];
        if (targetAction != "delete" && targetAction != "flag")
        {
            if (baseAction == "get")
            {
                var flag = JsonSerializer.Serialize(data.FlagThreshold, Core.Client.SerializerOptions);
                var del = JsonSerializer.Serialize(data.DeleteThreshold, Core.Client.SerializerOptions);
                embed.Description =
                    $"Flag Threshold\n```json\n{flag}\n```\nDelete Threshold\n```json\n{del}\n```";
                await message.Reply(embed);
            }
            else
            {
                await message.Reply($"Invalid action `{targetAction}`");
            }
            return;
        }


        var targetThreshold = targetAction switch
        {
            "delete" => data.DeleteThreshold,
            "flag" => data.FlagThreshold
        };
        
        string targetType = "";
        if (info.Arguments.Count > 3)
            targetType = info.Arguments[3];
        
        if (baseAction == "get")
        {
            switch (targetType)
            {
                case "adult":
                    embed.Description = $"`Adult: {targetThreshold.Adult}`";
                    break;
                case "spoof":
                    embed.Description = $"`Spoof: {targetThreshold.Spoof}`";
                    break;
                case "medical":
                    embed.Description = $"`Medical: {targetThreshold.Medical}`";
                    break;
                case "violence":
                    embed.Description = $"`Violence: {targetThreshold.Violence}`";
                    break;
                case "racy":
                    embed.Description = $"`Racy: {targetThreshold.Racy}`";
                    break;
                default:
                    if (targetAction == "delete")
                    {
                        embed.Description =
                            $"Delete Threshold\n```json\n{JsonSerializer.Serialize(data.DeleteThreshold, Client.SerializerOptionsLI)}\n```";
                    }
                    else if (targetAction == "flag")
                    {
                        embed.Description =
                            $"Flag Threshold\n```json\n{JsonSerializer.Serialize(data.FlagThreshold, Client.SerializerOptionsLI)}\n```";
                    }
                    break;
            }

            await message.Reply(embed);
            return;
        }

        int targetValue = -1;
        if (info.Arguments.Count > 4)
        {
            try
            {
                targetValue = int.Parse(info.Arguments[4]);
            }
            catch
            {
                await message.Reply($"Invalid value `{info.Arguments[4]}`");
                return;
            }
        }

        if (targetValue > 5 || targetValue < -1)
        {
            await message.Reply($"Target value can only be <=5 and >=-1");
            return;
        }

        switch (targetType)
        {
            case "adult":
                targetThreshold.Adult = targetValue;
                break;
            case "spoof":
                targetThreshold.Spoof = targetValue;
                break;
            case "medical":
                targetThreshold.Medical = targetValue;
                break;
            case "violence":
                targetThreshold.Violence = targetValue;
                break;
            case "racy":
                targetThreshold.Racy = targetValue;
                break;
        }
        
        // manually set the threshold fields depending
        // on the target action that we parsed.
        switch (targetAction)
        {
            case "delete":
                data.DeleteThreshold = targetThreshold;
                break;
            case "flag":
                data.FlagThreshold = targetThreshold;
                break;
        }

        try
        {
            await configController.Set(data);
        }
        catch (Exception ex)
        {
            embed.Description = $"Failed to save data. `{ex.Message}`";
            embed.Colour = CommandHelper.ErrorColor;
            await message.Reply(embed);
            await ReportError(ex, message, "Failed to save to ConfigController");
            return;
        }

        embed.Description = "Saved threshold data";
        embed.Colour = CommandHelper.DefaultColor;
        await message.Reply(embed);

    }
}