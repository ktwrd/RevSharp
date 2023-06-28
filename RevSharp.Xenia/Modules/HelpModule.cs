using Amazon.Runtime.Internal.Transform;
using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Modules;

[RevSharpModule]
public class HelpModule : CommandModule
{
    public override string? HelpCategory => null;
    public override bool HasHelpContent => false;
    public override string? BaseCommandName => "help";
    public override bool WaitForInit => false;
    public Dictionary<string, string> HelpDict = new();
    public Dictionary<string, string> OwnerHelpDict = new();

    public Dictionary<string, Dictionary<string, string>> HelpCategoryDictionary = new()
    {
        {
            "other", new Dictionary<string, string>()
        }
    };

    public Dictionary<string, Dictionary<string, string>> OwnerHelpCategoryDictionary = new()
    {
        {
            "other", new Dictionary<string, string>()
        }
    };
    public override Task InitComplete()
    {
        foreach (var item in Reflection.FetchModules<CommandModule>())
        {
            if (!item.HasHelpContent)
                continue;
            
            var key = item.BaseCommandName;
            if (item.BaseCommandName == BaseCommandName)
                continue;
            if (key == null)
                continue;
            
            var content = item.HelpContent();
            if (content == null)
                continue;
            if (item.OwnerOnly)
                OwnerHelpDict.TryAdd(key, content);
            else
                HelpDict.TryAdd(key, content);
            string category = (item.HelpCategory ?? "other")
                .Trim()
                .ToLower();

            if (item.OwnerOnly)
            {
                if (!OwnerHelpCategoryDictionary.ContainsKey(category))
                    OwnerHelpCategoryDictionary.Add(category, new Dictionary<string, string>());
                if (!OwnerHelpCategoryDictionary[category].ContainsKey(key))
                    OwnerHelpCategoryDictionary[category].Add(key, content);
            }
            else
            {
                if (!HelpCategoryDictionary.ContainsKey(category))
                    HelpCategoryDictionary.Add(category, new Dictionary<string, string>());
                if (!HelpCategoryDictionary[category].ContainsKey(key))
                    HelpCategoryDictionary[category].Add(key, content);
            }
        }        
        return Task.CompletedTask;
    }
    public override async Task CommandReceived(CommandInfo info, Message message)
    {
        if (info is not { Command: "help" })
            return;

        var embed = new SendableEmbed
        {
            Title = "Help",
            Colour = "white"
        };
        var isAdmin = Reflection.Config.OwnerUserIds.Contains(message.AuthorId);
        // No arguments given. Print out summary
        if (info.Arguments.Count < 1)
        {
            embed.Colour = "orange";
            embed.Description = HelpContent(isAdmin);
            await message.Reply(embed);
            return;
        }

        var helpDict = CombinedHelpDict(isAdmin);
        
        // 0th argument given is a valid module. print out summary
        if (helpDict.TryGetValue(info.Arguments[0], out var item))
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

    public Dictionary<string, string> CombinedHelpDict(bool inclAdmin)
    {
        var dict = new Dictionary<string, string>();
        foreach (var pair in HelpDict)
            dict.TryAdd(pair.Key, pair.Value);
        if (inclAdmin)
            foreach (var pair in OwnerHelpDict)
                dict.TryAdd(pair.Key, pair.Value);
        return dict;
    }

    public override string? HelpContent()
    {
        return HelpContent(false);
    }
    public string? HelpContent(bool inclOwner)
    {
        var p = Program.ConfigData.Prefix;
        var text = new List<string>()
        {
            $"To see the usage of a command. Run `{p}help <module>` and replace `<module>` with the name of the module you'd like to use.",
            $"For example, you can do `{p}help dice` to see the usage for the dice command.",
            "",
        };
        var dict = new Dictionary<string, Dictionary<string, string>>();

        void InsertDict(Dictionary<string, Dictionary<string, string>> d)
        {
            foreach (var x in d)
            {
                dict.TryAdd(x.Key, x.Value);
                foreach (var i in x.Value)
                {
                    dict[x.Key].TryAdd(i.Key, i.Value);
                }
            }
        }
        InsertDict(HelpCategoryDictionary);
        if (inclOwner)
            InsertDict(OwnerHelpCategoryDictionary);
        foreach (var categoryPair in dict)
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