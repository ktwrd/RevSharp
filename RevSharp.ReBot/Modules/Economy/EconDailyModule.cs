using RevSharp.Core.Models;
using RevSharp.ReBot.Helpers;
using RevSharp.ReBot.Models;
using RevSharp.ReBot.Reflection;

namespace RevSharp.ReBot.Modules;

[RevSharpModule]
public class EconDailyModule : BaseModule
{
    public override async Task MessageReceived(Message message)
    {
        var info = CommandHelper.FetchInfo(message);
        if (info == null || info.Command != "daily")
            return;

        var controller = Reflection.FetchModule<EconDataController>();
        var channel = await Client.GetChannel(message.ChannelId) as TextChannel;
        
        var embed = new SendableEmbed()
        {
            Title = "Economy Daily Rewards"
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
        
        // Check if last daily was more than 24hr ago.
        // If is was less than a day ago then we deny access to the user.
        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var timestampDiff = currentTimestamp - data.LastDailyTimestamp;
        if (timestampDiff < 86400)
        {
            var internalDiff = 86400 - timestampDiff;
            var second = Math.Round(internalDiff % 60f);
            var minute = Math.Round((internalDiff / 60f) % 60f);
            var hour = Math.Floor(minute % 60);
            var timeStr = "";
            if (hour > 0)
                timeStr += $"{hour} hour" + (hour > 1 ? "s " : " ");
            if (minute > 0)
                timeStr += $"{minute} minute" + (minute > 1 ? "s " : " ");
            if (second > 0)
                timeStr += $"{second} second" + (second > 1 ? "s" : "");
            embed.Description = $"Too fast! Try again in {timeStr}";
            embed.Colour = "orange";
            await message.Reply(embed);
            return;
        }

        // Increment coins and set last timestamp
        var inc = Program.Random.Next(10, 30);
        data.Coins += inc;
        data.LastDailyTimestamp = currentTimestamp;
        await controller.Set(data);
        
        embed.Description = string.Join("\n", new string[]
        {
            $"You gained `{inc}` coins!",
            "### Current Balance",
            $"`{data.Coins} coins`",
        });
        await message.Reply(embed);
    }
}