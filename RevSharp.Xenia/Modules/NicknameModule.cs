using RevSharp.Core;
using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Modules;

[RevSharpModule]
public class NicknameModule : CommandModule
{
    public override async Task CommandReceived(CommandInfo info, Message message)
    {
        string action = "";
        if (info.Arguments.Count > 0)
            action = info.Arguments[0];

        if (action == "help")
        {
            await Command_Help(info, message);
            return;
        }

        await Command_Set(info, message);
    }

    public override string? HelpContent()
    {
        return XeniaHelper.GenerateHelp(
            this, new List<(string, string)>()
            {
                (
                    "help", "display this message"
                ),
                (
                    "<user id> [<nickname>]", "set the nickname of the user. when no nickname provided it will reset their nickname."
                )
            });
    }

    public async Task Command_Help(CommandInfo info, Message message)
    {
        var embed = new SendableEmbed()
        {
            Title = "Nickname - Help",
            Colour = CommandHelper.DefaultColor,
            Description = HelpContent()
        };
        await message.Reply(embed);
    }

    public async Task Command_Set(CommandInfo info, Message message)
    {
        string? targetId = null;
        if (info.Arguments.Count > 0)
            targetId = CommandHelper.FindUserId(info.Arguments[0]);
        var embed = new SendableEmbed()
        {
            Title = "Nickname"
        };
        
        var server = await message.FetchServer();
        Member? member = null;
        try
        {
            member = await server.GetMember(targetId);
            if (member == null)
                throw new Exception("Member is null");
        }
        catch (RevoltException rex)
        {
            embed.Description = $"Failed to fetch member. `{rex.Message}`";
            embed.Colour = CommandHelper.ErrorColor;
            await message.Reply(embed);
            return;
        }
        catch (Exception ex)
        {
            embed.Description = $"Failed to fetch member. `{ex.Message}`";
            embed.Colour = CommandHelper.ErrorColor;
            await message.Reply(embed);
            await ReportError(ex, message, $"Failed to fetch member {targetId}");
            return;
        }

        string nickname = CommandHelper.FetchContent(info, 0);
        try
        {
            if (nickname.Length < 1)
            {
                if (!await member.Edit(
                    new DataMemberEdit()
                    {
                        Remove = new[]
                        {
                            "Nickname"
                        }
                    }))
                    throw new Exception("Unknown Error");
            }
            else
            {
                if (!await member.Edit(
                        new DataMemberEdit()
                        {
                            Nickname = nickname
                        }))
                    throw new Exception("Unknown Error");
            }
        }
        catch (RevoltException ex)
        {
            embed.Description = $"Failed to change nickname. `{ex.Message}`";
            embed.Colour = CommandHelper.ErrorColor;
            await message.Reply(embed);
            return;
        }
        catch (Exception ex)
        {
            embed.Description = $"Failed to change nickname. `{ex.Message}`";
            embed.Colour = CommandHelper.ErrorColor;
            await message.Reply(embed);
            await ReportError(ex, message, $"Failed to change nickname of {targetId} to `{nickname}`");
            return;
        }

        embed.Description = $"Successfully changed nickname";
        embed.Colour = CommandHelper.DefaultColor;
        await message.Reply(embed);
    }
    

    public override bool ServerOnly => true;
    public override bool HasHelpContent => true;
    public override string? HelpCategory => "moderation";
    public override string? BaseCommandName => "nick";
    public override bool WaitForInit => false;
    public override PermissionFlag? RequirePermission => PermissionFlag.ManageNicknames;
}