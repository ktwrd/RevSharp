using System.Diagnostics;
using System.Text.RegularExpressions;
using RevSharp.Core.Models;
using RevSharp.ReBot.Helpers;
using RevSharp.ReBot.Reflection;

namespace RevSharp.ReBot.Modules;

[RevSharpModule]
public class KickModule : BaseModule
{
    public override async Task MessageReceived(Message message)
    {
        #if !DEBUG
        return;
        #endif
        
        var info = CommandHelper.FetchInfo(message);
        if (info == null || info.Command != "kick")
            return;

        // Invalid amount of arguments
        if (info.Arguments.Count < 1)
        {
            
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

        var embed = new SendableEmbed()
        {
            Title = "Moderation - Kick"
        };
        
        var server = await message.FetchServer();
        if (server == null)
        {
            embed.Description = "Failed to fetch server!";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
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
}