using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;

namespace RevSharp.Xenia.Modules;

public partial class ConDetectConfigModule
{
    public async Task Command_Help(CommandInfo info, Message message)
    {
        await message.AddReaction(Client, "✅");
        var action = "";
        if (info.Arguments.Count > 1)
            action = info.Arguments[1].ToLower();


        var embed = new SendableEmbed()
        {
            Title = "Content Detection Config - Help",
            Colour = CommandHelper.DefaultColor
        };
        if (action.Length < 1)
        {
            embed.Description = HelpContent();
            await message.Reply(embed);
            return;
        }

        if (ActionHelp.TryGetValue(action, out var value))
        {
            var content = string.Join("\n", value);
            embed.Description = content;
            await message.Reply(embed);
            return;
        }

        embed.Description = $"Invalid action `{action}`";
        embed.Colour = CommandHelper.ErrorColor;
        await message.Reply(embed);
    }
}