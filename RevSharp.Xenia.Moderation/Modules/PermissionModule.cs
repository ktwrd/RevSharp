using RevSharp.Core;
using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Moderation.Modules;

[RevSharpModule]
public class PermissionModule : CommandModule
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
            case "user":
                await Command_User(info, message);
                break;
            default:
                await message.Reply($"Unknown action `{action,-1}`. See `{Reflection.Config.Prefix}{BaseCommandName} help`");
                break;
        }
    }

    public async Task Command_User(CommandInfo info, Message message)
    {
        await message.AddReaction(Client, "✅");
        string? targetUserId = message.AuthorId;
        if (info.Arguments.Count > 1)
            targetUserId = CommandHelper.FindUserId(info.Arguments[1]);
        var embed = new SendableEmbed()
        {
            Title = "Permission - User"
        };
        var server = await message.FetchServer();
        Member? member = null;
        try
        {
            member = await server.GetMember(targetUserId);
            if (member == null)
                throw new RevoltException("NotFound");
        }
        catch (RevoltException rex)
        {
            embed.Description = $"Failed to fetch server member: `{rex.Message}`";
            embed.Colour = CommandHelper.ErrorColor;
            await message.Reply(embed);
            return;
        }
        catch (Exception ex)
        {
            embed.Description = $"Failed to fetch server member: `{ex.Message}`";
            embed.Colour = CommandHelper.ErrorColor;
            await message.Reply(embed);
            await ReportError(ex, message, $"Failed to fetch member `{targetUserId}` in server `{server.Id}`");
            return;
        }

        var permissions = await member.GetPermissions(Client, false);
        embed.Description = string.Join(", ", permissions.Select(v => $"`{v}`"));
        embed.Colour = CommandHelper.DefaultColor;
        await message.Reply(embed);
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
            ("help", "show help content for this command"),
            ("user [<user>]", "Get summary on what a user can and cannot do. When parameter `user` is not provided, then it will default to the author (you)")
        });
    }

    public override bool HasHelpContent => false;
    public override string? HelpCategory => "mod";
    public override string? BaseCommandName => "permission";
    public override bool WaitForInit => false;
    public override bool ServerOnly => true;
}