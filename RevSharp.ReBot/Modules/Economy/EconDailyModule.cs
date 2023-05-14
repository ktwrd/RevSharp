using RevSharp.Core.Models;
using RevSharp.ReBot.Helpers;
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

        var controller = Reflection.FetchModule<EconController>();
        if (controller == null)
            return;

        var channel = await Client.GetChannel(message.ChannelId) as TextChannel;
        var data = controller.GetUser(message.AuthorId, channel.ServerId)
                   ?? new EconProfile()
                   {
                       UserId = message.AuthorId,
                       ServerId = channel.ServerId,
                       LastDailyTimestamp = 0
                   };
        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var timestampDiff = currentTimestamp - data.LastDailyTimestamp;
        if (timestampDiff < 86400)
        {
            var timeStr = TimeHelper.SinceTimestamp(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() -
                                                    ((86400 - timestampDiff) * 1000));
            await message.Reply($"Too fast! Try again in {timeStr}");
            return;
        }

        data.LastDailyTimestamp = currentTimestamp;

        var inc = Program.Random.Next(10, 30);
        data.Coins += inc;
        await message.Reply($"You gained `{inc}` coins!\nCurrent Balance: {data.Coins}");
        controller.SetUser(data);
    }
}