using RevSharp.Core.Models;
using RevSharp.Skidbot.Helpers;
using RevSharp.Skidbot.Models;
using RevSharp.Skidbot.Reflection;

namespace RevSharp.Skidbot.Modules;
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
        var p = Program.ConfigData.Prefix;
        return string.Join("\n", new string[]
        {
            "View your balance for this server",
            "```",
            $"{p}bal        - View balance",
            $"{p}balance    - View Balance",
            "```"
        });
    }

    public override bool HasHelpContent => true;
    public override string? InternalName => "balance";
    public override string? HelpCategory => "economy";
}