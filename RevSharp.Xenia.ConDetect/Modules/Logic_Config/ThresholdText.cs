using System.Text.Json;
using kate.shared.Helpers;
using RevSharp.Core.Models;
using RevSharp.Xenia.GoogleCloud.Perspective.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Models.ContentDetection;

namespace RevSharp.Xenia.Modules;

public partial class ConDetectConfigModule
{
    public async Task Command_ThresholdText(CommandInfo info, Message message)
    {
        await message.AddReaction(Client, "✅");
        var embed = new SendableEmbed()
        {
            Title = "Content Detection Config - Text Threshold"
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
                var flag = JsonSerializer.Serialize(data.TextFlagThreshold, Core.Client.SerializerOptions);
                var del = JsonSerializer.Serialize(data.TextDeleteThreshold, Core.Client.SerializerOptions);
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
            "delete" => data.TextDeleteThreshold,
            "flag" => data.TextFlagThreshold
        };
        
        string targetType = "";
        if (info.Arguments.Count > 3)
            targetType = info.Arguments[3];
        
        CommentAttributeName? parsedTypeMaybeNull = null;
        try
        {
            if (targetType.Length > 0)
                parsedTypeMaybeNull = Enum.Parse<CommentAttributeName>(targetType.ToUpper());
        }
        catch (Exception ex)
        {
            embed.Description = $"Failed to parse type, `{targetType,-1}`\n`{ex.Message}`";
            embed.Colour = CommandHelper.ErrorColor;
            await message.Reply(embed);
            await ReportError(ex, message, "Failed to parse enum type");
            return;
        }

        if (parsedTypeMaybeNull == null)
        {
            embed.Description = $"Invalid type `{targetType,-1}`";
            embed.Colour = CommandHelper.ErrorColor;
            await message.Reply(embed);
            return;
        }

        CommentAttributeName parsedType = (CommentAttributeName)parsedTypeMaybeNull;
        
        var typeEnumMembers = GeneralHelper.GetEnumList<CommentAttributeName>();
        if (baseAction == "get")
        {
            if (targetType.Length < 1)
            {
                if (targetAction == "delete")
                {
                    embed.Description =
                        $"Delete Threshold\n```json\n{JsonSerializer.Serialize(data.TextDeleteThreshold, Client.SerializerOptionsLI)}\n```";
                }
                else if (targetAction == "flag")
                {
                    embed.Description =
                        $"Flag Threshold\n```json\n{JsonSerializer.Serialize(data.TextFlagThreshold, Client.SerializerOptionsLI)}\n```";
                }
            }
            else if (typeEnumMembers.Select(v => v.ToString()).Contains(targetType.ToUpper()))
            {
                if (targetThreshold.TryGetValue(parsedType.ToString(), out var thresholdValue))
                {
                    embed.Description = $"`{parsedType} = {thresholdValue}`";
                }
                else
                {
                    embed.Description = $"Invalid type `{targetType,-1}`";
                    embed.Colour = CommandHelper.ErrorColor;
                }
            }
            await message.Reply(embed);
            return;
        }

        float targetValue = -1;
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

        if (targetValue > 1f || targetValue < -1f)
        {
            await message.Reply($"Target value can only be between 1 and 0 or -1");
            return;
        }

        targetThreshold.TryAdd(parsedType.ToString(), targetValue);
        targetThreshold[parsedType.ToString()] = targetValue;

        switch (targetAction)
        {
            case "delete":
                data.TextDeleteThreshold = targetThreshold;
                break;
            case "flag":
                data.TextFlagThreshold = targetThreshold;
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