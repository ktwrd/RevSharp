using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Modules;

[RevSharpModule]
public class DiceModule : CommandModule
{
    public override string? HelpCategory => null;
    public override bool HasHelpContent => true;
    public override string? BaseCommandName => "dice";
    public override bool WaitForInit => false;

    public override string? HelpContent()
    {
        var p = Reflection.Config.Prefix;
        return string.Join("\n", new string[]
        {
            "```",
            $"{p}dice <min> <max>     - generate random number (x-y)",
            $"{p}dice <max>           - generate random number (0-x)",
            $"{p}dice help            - display this message",
            "",
            "max: Maximum number for dice roll",
            "min: Minimum number for dice roll (default: 0)",
            "```"
        });
    }

    public override async Task CommandReceived(CommandInfo info, Message message)
    {
        if (info is not {Command: "dice"})
            return;

        var numberRegex = new Regex(@"^[0-9]+$");
        int min = 0;
        int max = 0;
        var embed = new SendableEmbed()
        {
            Title = "Dice"
        };
        if (info.Arguments.Count < 1 || (info.Arguments.Count > 0  && info.Arguments[0].ToLower() == "help"))
        {
            embed.Title += " - Usage";
            embed.Description = HelpContent();
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }

        
        if (!numberRegex.IsMatch(info.Arguments[0]))
        {
            embed.Title += " - Error";
            embed.Description = $"Invalid Argument `{info.Arguments[0]}`";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }

        if (info.Arguments.Count > 1)
        {
            if (!numberRegex.IsMatch(info.Arguments[1]))
            {
                embed.Title += " - Error";
                embed.Description = $"Invalid Argument `{info.Arguments[1]}`";
                embed.Colour = "red";
                await message.Reply(embed);
                return;
            }
        }
        if (info.Arguments.Count < 2)
        {
            max = int.Parse(info.Arguments[0]);
        }
        else if (info.Arguments.Count < 3)
        {
            min = int.Parse(info.Arguments[0]);
            max = int.Parse(info.Arguments[1]);
        }

        int result = _random.Next(min, max);
        embed.Description = $"## {result}";
        await message.Reply(embed);
    }

    private readonly Random _random = new();
}