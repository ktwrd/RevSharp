using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Moderation.Modules;

[RevSharpModule]
public class ModLogModule : CommandModule
{
    public override async Task CommandReceived(CommandInfo info, Message message)
    {
        string action = "";
        if (info.Arguments.Count > 0)
            action = info.Arguments[0].ToLower();

        switch (action)
        {
            case "help":
                await Command_Help(info, message);
                break;
            default:
                await message.Reply($"Unknown action `{action,-1}`. See `{Reflection.Config.Prefix}{BaseCommandName} help`");
                break;
        }
    }

    public async Task Command_Help(CommandInfo info, Message message)
    {
        var embed = new SendableEmbed()
        {
            Title = "Help Content",
            Description = HelpContent(),
            Colour = CommandHelper.DefaultColor
        };
        await message.Reply(embed);
    }
    
    public override string? HelpContent()
    {
        return XeniaHelper.GenerateHelp(this, new List<(string, string)>()
        {
            ("help", "show help content for this command")
        });
    }

    public override bool HasHelpContent => false;
    public override string? HelpCategory => "moderation";
    public override string? BaseCommandName => "modlog";
    public override bool WaitForInit => false;
    public override PermissionFlag? RequirePermission => PermissionFlag.ManageServer;
    public override bool ServerOnly => true;
}