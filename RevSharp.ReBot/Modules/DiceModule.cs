using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using RevSharp.Core.Models;
using RevSharp.ReBot.Helpers;
using RevSharp.ReBot.Reflection;

namespace RevSharp.ReBot.Modules;

[RevSharpModule]
public class DiceModule : BaseModule
{
    public override async Task Initialize(ReflectionInclude reflection)
    {
        var help = reflection.FetchModule<HelpModule>();
        if (help != null)
            help.HelpDict.Add("dice", string.Join("\n", new string[]
            {
                "```",
                "r.dice <min> <max>     - generate random number (x-y)",
                "r.dice <max>           - generate random number (0-x)",
                "r.dice help            - display this message",
                "",
                "max: Maximum number for dice roll",
                "min: Minimum number for dice roll (default: 0)",
                "```"
            }));
    }
    public override async Task MessageReceived(Message message)
    {
        var info = CommandHelper.FetchInfo("r.", message.Content);
        if (info == null)
            return;
        if (info.Command != "dice")
            return;
        var help = Reflection.FetchModule<HelpModule>();

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
            embed.Description = help.HelpDict["dice"];
            embed.Colour = "red";
            await message.Reply("", embeds: new[] { embed });
            return;
        }

        
        if (!numberRegex.IsMatch(info.Arguments[0]))
        {
            embed.Title += " - Error";
            embed.Description = $"Invalid Argument `{info.Arguments[0]}`";
            embed.Colour = "red";
            await message.Reply("", embeds: new[] { embed });
            return;
        }

        if (info.Arguments.Count > 1)
        {
            if (!numberRegex.IsMatch(info.Arguments[1]))
            {
                embed.Title += " - Error";
                embed.Description = $"Invalid Argument `{info.Arguments[1]}`";
                embed.Colour = "red";
                await message.Reply("", embeds: new[] { embed });
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

        int result = Program.Random.Next(min, max);
        embed.Description = $"## {result}";
        await message.Reply("", embeds: new[] { embed });
    }
}