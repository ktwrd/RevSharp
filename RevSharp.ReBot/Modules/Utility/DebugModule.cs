using System.Text.RegularExpressions;
using RevSharp.Core.Models;
using RevSharp.ReBot.Helpers;
using RevSharp.ReBot.Reflection;

namespace RevSharp.ReBot.Modules.Utility;

[RevSharpModule]
public class DebugModule : BaseModule
{
    public override string? HelpCategory => "utility";
    public override string? InternalName => "debug";
    public override bool HasHelpContent => true;

    public override string HelpContent()
    {
        var p = Program.ConfigData.Prefix;
        return string.Join("\n", new string[]
        {
            "```",
            $"{p}debug help     - display this command",
            $"{p}debug snowflake- get debug info about a ulid or snowflake",
            "```"
        });
    }

    private async Task Command_Snowflake(CommandInfo info, Message message)
    {
        var ulidRegex = new Regex(@"^[0-9A-HJ-KM-NP-TV-Z]{26}$", RegexOptions.IgnoreCase);
        string arg = "";
        if (info.Arguments.Count > 1)
            arg = info.Arguments[1];
        if (ulidRegex.IsMatch(arg))
        {
            var decoded = Ulid.Parse(arg);
            var tsSmall = Math.Round(decoded.Time.ToUnixTimeMilliseconds() / 1000f);
            await message.Reply(
                new SendableEmbed()
                {
                    Description = string.Join(
                        "\n", new string[]
                        {
                            $"`ULID: {arg}`", $"`Timestamp: {decoded.Time}` (<t:{tsSmall}:F> / <t:{tsSmall}:R>)"
                        })
                });
            return;
        }

        await message.Reply("invalid argument lol");
    }
    private async Task Command_Help(CommandInfo info, Message message)
    {
        var embed = new SendableEmbed()
        {
            Title = "Permission Cloning - Help"
        };
        embed.Description = HelpContent();
        await message.Reply(embed);
    }
    public override async Task MessageReceived(Message message)
    {
        var info = CommandHelper.FetchInfo(message);
        if (info?.Command != InternalName)
            return;

        var action = "help";
        if (info.Arguments.Count > 0)
            action = info.Arguments[0].ToLower();
        switch (action)
        {
            case "snowflake":
                await Command_Snowflake(info, message);
                break;
            default:
                await Command_Help(info, message);
                break;
            case "help":
                await Command_Help(info, message);
                break;
        }
    }
}