using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
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
            
            default:
                await message.Reply($"Invalid action `{action}`");
                break;
        }
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