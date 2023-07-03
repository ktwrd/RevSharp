using System.Text.Json;
using kate.shared.Helpers;
using RevSharp.Core;
using RevSharp.Core.Models;
using RevSharp.Xenia.GoogleCloud.Perspective.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Models.ContentDetection;
using RevSharp.Xenia.Reflection;
using Emoji = GEmojiSharp.Emoji;
namespace RevSharp.Xenia.Modules;

[RevSharpModule]
public partial class ConDetectConfigModule : CommandModule
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
            case "thresholdtxt":
                await Command_ThresholdText(info, message);
                break;
            case "ignore":
                await Command_Ignore(info, message);
                break;
            default:
                await message.Reply($"Invalid action `{action}`");
                break;
        }
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
                $">`{Prefix} usedefault <delete|flag>`",
                ">Use the global default settings for content detection.",
                "`delete` for resetting the delete threshold, and `flag` for the flag message threshold."
            }
        },
        {
            "logchannel",
            new string[]
            {
                $">`{Prefix} logchannel <channel> <action> <type>`",
                ">Set the log channel for content detection.",
                "When channel is not specified, it will default to the channel that you're sending the message in.",
                "If `channel` is `this`, it will use the current channel. By default it will do this if no",
                "parameter is given in this place",
                "- action must be one of the following",
                "  - `action=any` will override all channel id's to the one given or the current one",
                "  - `action=delete` will override the delete action for the type",
                "  - `action=flag` will override the flag action for this type",
                "- type must be one of the following",
                "  - `type=any` will override all channel id's to the one given or the current one",
                "  - `type=text` text detection events",
                "  - `type=media` media detection events",
                "default value for `type` and `action` is `any`"
            }
        },
        {
            "threshold",
            new string[]
            {
                $">`{Prefix} threshold <func=get|set> <action=delete|flag> <type> <value>`",
                 ">Set threshold for specific types",
                $"- when `action` isn't valid or empty and the `func` parameter is `get`, it will print both `delete` and `flag` thresholds.",
                $"- `value` must be >= -1 or <= 5",
                $"- `type` must be one of the following;",
                 "- `adult, spoof, medical, violence, racy`"
            }
        },
        {
            "thresholdtxt",
            new string[]
            {
                $">`{Prefix} thresholdtxt <func=get|set> <action=delete|flag> <type> <value>`",
                 ">Set threshold for text messages.",
                $"- when `action` isn't valid or empty and the `func` parameter is `get`, it will print both `delete` and `flag` thresholds.",
                $"- `value` must be >=0 or <=5. when `-1` then it will ignore",
                $"- `type` must be one of the following (it's case-insensitive)",
                "- " + string.Join(", ", GeneralHelper.GetEnumList<CommentAttributeName>().Select(v => $"`{v}`"))
            }
        },
        {
            "ignore",
            new string[]
            {
                $">`{Prefix} ignore <type=user|channel> <id|mention>`",
                 "> Ignore a user or a channel from Content Detection"
            }
        }
    };

    public override bool HasHelpContent => true;
    public override string? HelpCategory => "moderation";
    public override string? BaseCommandName => "cdconfig";
    public override PermissionFlag? RequirePermission => PermissionFlag.ManageServer;
    public override bool ServerOnly => true;
}