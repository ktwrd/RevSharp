using RevSharp.Core.Models;
using RevSharp.ReBot.Helpers;
using RevSharp.ReBot.Reflection;

namespace RevSharp.ReBot.Modules;

[RevSharpModule]
public class HelpModule : BaseModule
{
    public override bool HasHelpContent => false;
    public override string? InternalName => "help";
    public Dictionary<string, string> HelpDict = new();

    public Dictionary<string, Dictionary<string, string>> HelpCategoryDictionary = new()
    {
        {"other", new Dictionary<string, string>()}
    };
    public override Task Initialize(ReflectionInclude reflection)
    {
        foreach (var item in reflection.FetchModules())
        {
            if (!item.HasHelpContent)
                continue;
            
            var key = item.InternalName;
            if (key == null)
                continue;
            
            var content = item.HelpContent();
            if (content == null)
                continue;

            HelpDict.TryAdd(key, content);
            string category = (item.HelpCategory ?? "other")
                .Trim()
                .ToLower();
            
            Console.WriteLine($"contains: {category} {HelpCategoryDictionary.ContainsKey(category)}");
            if (!HelpCategoryDictionary.ContainsKey(category))
                HelpCategoryDictionary.Add(category, new Dictionary<string, string>());
            Console.WriteLine($"contains: {category} {HelpCategoryDictionary.ContainsKey(category)}");
            if (!HelpCategoryDictionary[category].ContainsKey(key))
                HelpCategoryDictionary[category].Add(key, content);
        }        
        return Task.CompletedTask;
    }
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
            embed.Colour = "orange";
            embed.Description = HelpContent();
            await message.Reply(embed);
            return;
        }

        // 0th argument given is a valid module. print out summary
        if (HelpDict.TryGetValue(info.Arguments[0], out var item))
        {
            embed.Description = item;
            await message.Reply(embed);
        }
        // 0th argument given isn't a valid module. print error
        else
        {
            embed.Description = $"Module `{info.Arguments[0]}` not found";
            embed.Colour = "red";
            await message.Reply(embed);
        }
    }

    public override string? HelpContent()
    {
        var text = new List<string>()
        {
            "To see the usage of a command. Run `r.help <module>` and replace `<module>` with the name of the module you'd like to use.",
            "For example, you can do `r.help dice` to see the usage for the dice command.",
            "",
        };
        foreach (var categoryPair in HelpCategoryDictionary)
        {
            // ignore empty categories
            if (categoryPair.Value.Count < 1)
                continue;
            var key = categoryPair.Key;
            var formattedKey = "";
            char? previousIndex = null;
            for (int i = 0; i < key.Length; i++)
            {
                char target = key[i];
                if (target == '_')
                    target = ' ';

                if (previousIndex == null || previousIndex == ' ')
                    target = char.ToUpper(target);

                formattedKey += target;
                previousIndex = target;
            }
            text.Add($"### {formattedKey}");
            var inner = categoryPair.Value
                .Select(v => $"- `{v.Key}`");
            text.AddRange(inner);
        }

        return string.Join("\n", text);
    }
}