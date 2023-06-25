using System.Text.Json;
using RevSharp.Core;
using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Models.ContentDetection;
using RevSharp.Xenia.Reflection;
using Emoji = GEmojiSharp.Emoji;
namespace RevSharp.Xenia.Modules;

[RevSharpModule]
public class ConDetectConfigModule : BaseModule
{
    public override async Task CommandReceived(CommandInfo info, Message message)
    {
        await message.AddReaction(Client, Emoji.Get("white_check_mark").Raw);
        var action = "";
        if (info.Arguments.Count > 0)
            action = info.Arguments[0].ToLower();

        switch (action)
        {
            case "":
                await Command_Help(info, message);
                break;
            case "help":
                await Command_Help(info, message);
                break;
            case "usedefault":
                await Command_UseDefault(info, message);
                break;
            case "logchannel":
                await Command_LogChannel(info, message);
                break;
            case "threshold":
                await Command_Threshold(info, message);
                break;
            default:
                await message.Reply($"Invalid action `{action}`");
                break;
        }
    }

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
                var flag = JsonSerializer.Serialize(data.FlagThreshold, Client.SerializerOptions);
                var del = JsonSerializer.Serialize(data.DeleteThreshold, Client.SerializerOptions);
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
                            $"Delete Threshold\n```json\n{JsonSerializer.Serialize(data.DeleteThreshold, Client.SerializerOptions)}\n```";
                    }
                    else if (targetAction == "flag")
                    {
                        embed.Description =
                            $"Flag Threshold\n```json\n{JsonSerializer.Serialize(data.FlagThreshold, Client.SerializerOptions)}\n```";
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
    public async Task Command_LogChannel(CommandInfo info, Message message)
    {
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

        data.LogChannelId = targetChannelId;
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

        embed.Description = $"Set log channel to <#{message.ChannelId}>";
        embed.Colour = CommandHelper.DefaultColor;
        await message.Reply(embed);
    }
    
    public async Task Command_UseDefault(CommandInfo info, Message message)
    {
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

    public async Task Command_Help(CommandInfo info, Message message)
    {
        var action = "";
        if (info.Arguments.Count > 1)
            action = info.Arguments[1].ToLower();


        var embed = new SendableEmbed()
        {
            Title = "Content Detection Config - Help",
            Colour = CommandHelper.DefaultColor
        };
        if (action.Length < 1)
        {
            embed.Description = HelpContent();
            await message.Reply(embed);
            return;
        }

        if (ActionHelp.TryGetValue(action, out var value))
        {
            var content = string.Join("\n", value);
            embed.Description = content;
            await message.Reply(embed);
            return;
        }

        embed.Description = $"Invalid action `{action}`";
        embed.Colour = CommandHelper.ErrorColor;
        await message.Reply(embed);
    }
    
    public override string HelpContent()
    {
        var subActions = ActionHelp.Select(v => v.Key);
        return string.Join(
            "\n", new string[]
            {
                "Available actions;",
                string.Join("\n", subActions.Select(v => $"- `{v}`")),
                $"Use `{Prefix} help <action>` to view more details"
            });
    }

    private string Prefix => Reflection.Config.Prefix + BaseCommandName;
    public Dictionary<string, string[]> ActionHelp => new Dictionary<string, string[]>()
    {
        {
            "usedefault",
            new string[]
            {
                $">`{Prefix} usedefault [delete|flag]`",
                "Use the global default settings for content detection.",
                "`delete` for resetting the delete threshold, and `flag` for the flag message threshold."
            }
        }
    };

    public override bool HasHelpContent => false;
    public override string? InternalName => "condetect_config";
    public override string? HelpCategory => "moderation";
    public override string? BaseCommandName => "cdconfig";
    public override PermissionFlag? RequireServerPermission => PermissionFlag.ManageServer;
}