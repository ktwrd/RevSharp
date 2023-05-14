using RevSharp.Core.Models;
using RevSharp.ReBot.Helpers;
using RevSharp.ReBot.Reflection;

namespace RevSharp.ReBot.Modules;

[RevSharpModule]
public class HelpModule : BaseModule
{
    public Dictionary<string, string> HelpDict => new();
    public override async Task MessageReceived(Message message)
    {
        var info = CommandHelper.FetchInfo("r.", message.Content);
        if (info is not { Command: "help" })
            return;

        var embed = new SendableEmbed
        {
            Title = "Help",
            Colour = "white"
        };
        
        // No arguments given. Print out summary
        if (info.Arguments.Count < 1)
        {
            var text = new List<string>()
            {
                "To see the usage of a command. Run `r.help <module>` and replace `<module>` with the name of the module you'd like to use",
                "",
                "## Modules",
            };
            foreach (var pair in HelpDict)
            {
                text.Add($"- `{pair.Key}`");
            }

            embed.Colour = "orange";
            embed.Description = string.Join("\n", text);
            await message.Reply("", embeds: new[] { embed });
            return;
        }

        // 0th argument given is a valid module. print out summary
        if (HelpDict.TryGetValue(info.Arguments[0], out var item))
        {
            embed.Description = item;
            await message.Reply("", embeds: new[] { embed });
        }
        // 0th argument given isn't a valid module. print error
        else
        {
            embed.Description = $"Module `{info.Arguments[0]}` not found";
            embed.Colour = "red";
            await message.Reply("", embeds: new[] { embed });
        }
    }
}