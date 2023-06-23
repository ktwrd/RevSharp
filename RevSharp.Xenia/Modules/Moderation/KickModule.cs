using System.Diagnostics;
using System.Text.RegularExpressions;
using RevSharp.Core;
using RevSharp.Core.Helpers;
using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Modules;

[RevSharpModule]
public class KickModule : BaseModule
{
    public override async Task CommandReceived(CommandInfo info, Message message)
    {
        #if !DEBUG
        return;
        #endif
        
        if (info.Command != "kick")
            return;

        var embed = new SendableEmbed()
        {
            Title = "Moderation - Kick"
        };
        
        // Invalid amount of arguments
        if (info.Arguments.Count < 1)
        {
            embed.Title += " - Usage";
            embed.Description = HelpContent();
            embed.Colour = "orange";
            await message.Reply(embed);
            return;
        }
        
        var server = await message.FetchServer();
        if (server == null)
        {
            embed.Description = "Failed to fetch server!";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }

        try
        {
            var authorMember = await server.GetMember(message.AuthorId, forceUpdate: false);
            if (!await authorMember.HasPermission(PermissionFlag.KickMembers, forceUpdate: false))
            {
                embed.Description = "You do not have access to this command";
                embed.Colour = "red";
                await message.Reply(embed);
                return;
            }

            var selfMember = await server.GetMember(Client.CurrentUserId, forceUpdate: false);
            if (!await selfMember.HasPermission(PermissionFlag.KickMembers, forceUpdate: false))
            {
                embed.Description = "I do not have permission to kick this member!";
                embed.Colour = "red";
                await message.Reply(embed);
                return;
            }
        }
        catch (RevoltException e)
        {
            embed.Description = string.Join("\n", new string[]
            {
                "Failed to run module!",
                "```",
                e.Message,
                "```"
            });
            embed.Colour = "red";
            await message.Reply(embed);
            throw;
            return;
        }

        string targetId = "";
        var userIdRegex = new Regex(@"^[a-zA-Z0-9]{26}$");
        if (userIdRegex.IsMatch(info.Arguments[0].ToLower()))
        {
            targetId = info.Arguments[0].ToUpper();
        }
        var userMentionRegex = new Regex(@"^<@([a-zA-Z0-9]{26})>$");
        if (targetId.Length < 1 && userMentionRegex.IsMatch(info.Arguments[0].ToLower()))
        {
            var match = userMentionRegex.Match(info.Arguments[0].ToLower());
            if (match.Groups.Count >= 2)
            {
                targetId = match.Groups[1].Value.ToUpper();
            }
        }

        if (targetId.Length < 1)
        {
            embed.Description = "Failed to find User ID";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }

        // var innerList = new List<string>(info.Arguments);
        // innerList.RemoveAt(0);
        // string reason = "";
        // if (info.Arguments.Count > 1)
        // {
        //     var innerArgumentList = new List<string>(info.Arguments);
        //     innerArgumentList.RemoveAt(0);
        //     reason = string.Join(" ", innerArgumentList);
        // }

        bool success = false;
        try
        {
            success = await server.KickMember(targetId);
        }
        catch (MissingPermissionException missingPermissionException)
        {
            embed.Description = string.Join("\n", new string[]
            {
                "Failed to kick member;",
                $"Missing Permission `{missingPermissionException.Permission}`"
            });
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }
        catch (RevoltException revoltException)
        {
            embed.Description = string.Join("\n", new string[]
            {
                $"Failed to kick member. (Reason: `{revoltException.Message}`)"
            });
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }
        catch (Exception e)
        {
            embed.Description = string.Join("\n", new string[]
            {
                "Failed to Kick",
                "```",
                e.ToString(),
                "```"
            });
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }

        if (success)
        {
            embed.Description = "Successfully kicked member";
            embed.Colour = "green";
        }
        else
        {
            embed.Description = "Failed to kick member";
            embed.Colour = "red";
        }
        await message.Reply(embed);
    }

    public override string? HelpContent()
    {
        var p = Program.ConfigData.Prefix;
        return string.Join("\n", new string[]
        {
            "```",
            $"{p}kick <user id>      - Kick member via user ID",
            $"{p}kick <mention>      - Kick member via mention",
            " ---- Example ----",
            $"{p}kick <@01GZD4F61RBPJ5HWD08XB8F78N>",
            "```"
        });
    }

    public override bool HasHelpContent =>
    #if DEBUG
        true;
    #else
        false;
    #endif
    public override string? InternalName => "kick";
    public override string? HelpCategory => "moderation";
    public override string? BaseCommandName => "kick";
    public override bool WaitForInit => false;
}