using RevSharp.Core.Models;
using RevSharp.ReBot.Helpers;
using RevSharp.ReBot.Models;
using RevSharp.ReBot.Reflection;

namespace RevSharp.ReBot.Modules;
[RevSharpModule]
public class EconBalanceModule : BaseModule
{
    public override async Task MessageReceived(Message message)
    {
        var info = CommandHelper.FetchInfo(message);
        if (info == null || (info.Command != "bal" && info.Command != "balance"))
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
        return string.Join("\n", new string[]
        {
            "View your balance for this server",
            "```",
            "r.bal        - View balance",
            "r.balance    - View Balance",
            "```"
        });
    }

    public override bool HasHelpContent => true;
    public override string? InternalName => "balance";
    public override string? HelpCategory => "economy";
}