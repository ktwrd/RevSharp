using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Models;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Modules;
[RevSharpModule]
public class EconBalanceModule : CommandModule
{
    public override async Task CommandReceived(CommandInfo info, Message message)
    {
        if (info.Command != "balance")
            return;
        
        var controller = Reflection.FetchModule<EconDataController>();
        var channel = await Client.GetChannel(message.ChannelId) as TextChannel;
        
        var embed = new SendableEmbed()
        {
            Title = "Economy Balance"
        };
        
        // Fetch or create Economy Profile
        var data =
            await controller.Get(message.AuthorId, channel.ServerId)
            ?? new EconProfileModel()
            {
                UserId = message.AuthorId,
                ServerId = channel.ServerId,
                LastDailyTimestamp = 0
            };

        embed.Description = $"Balance: `{data.Coins} coins`";
        await message.Reply(embed);
    }
    
    public override string? HelpContent()
    {
        var p = Program.ConfigData.Prefix;
        return string.Join("\n", new string[]
        {
            "View your balance for this server",
            "```",
            $"{p}balance    - View Balance",
            "```"
        });
    }

    public override bool HasHelpContent => true;
    public override string? BaseCommandName => "balance";
    public override string? HelpCategory => "economy";
}