using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Models.ContentDetection;

namespace RevSharp.Xenia.Modules;

public partial class ContentDetectionModule
{
    private async Task Command_Admin(CommandInfo info, Message message)
    {
        if (!Reflection.Config.OwnerUserIds.Contains(message.AuthorId))
            return;

        if (info.Arguments.Count < 3)
        {
            var r = Reflection.Config.Prefix + BaseCommandName;
            await message.Reply(
                new SendableEmbed()
                {
                    Title = "Content Detection Admin - Help",
                    Description = string.Join("\n",
                        new string[]
                        {
                            $"```",
                            $"{r} admin allow <id>            - Allow server to use CD",
                            $"{r} admin deny <id> <...reason> - Deny server to use CD",
                            $"{r} admin deny <id> <...reason> - Ban server to use CD",
                            "```"
                        })
                });
            return;
        }
        
        var action = info.Arguments[1].ToLower();
        var id = info.Arguments[2].ToUpper();
        var reason = "<none>";
        if (info.Arguments.Count > 3)
        {
            var theRest = info.Arguments.ToList();
            theRest.RemoveRange(0, 3);
            reason = string.Join(" ", theRest);
        }

        var controller = Reflection.FetchModule<ContentDetectionServerConfigController>();
        var d = await controller.Fetch(id) ??
                new AnalysisServerConfig()
                {
                    ServerId = id
                };
        
        switch (action)
        {
            case "allow":
                var allowRes = await AllowServer(id);
                await message.Reply(new SendableEmbed()
                {
                    Title = "Content Detection Admin - Update State",
                    Description = $"State attempt set to allowed. Result: `{allowRes}`"
                });
                break;
            case "deny":
                var denyRes = await DenyServer(id, reason);
                await message.Reply(new SendableEmbed()
                {
                    Title = "Content Detection Admin - Update State",
                    Description = $"State attempt set to denied. Result: `{denyRes}`"
                });
                break;
            case "ban":
                var banRes = await BanServer(id, reason);
                await message.Reply(new SendableEmbed()
                {
                    Title = "Content Detection Admin - Update State",
                    Description = $"State attempt set to banned. Result: `{banRes}`"
                });
                break;
        }
    }

    private async Task<string?> AllowServer(string serverId)
    {
        var controller = Reflection.FetchModule<ContentDetectionServerConfigController>();
        var d = await controller.Fetch(serverId);
        if (d == null)
            return "ServerNotFound";

        d.Enabled = true;
        d.AllowAnalysis = true;
        d.HasRequested = false;
        var channel = await Client.GetChannel(d.LogChannelId) as TextChannel;
        if (channel == null)
            return "ChannelNotFound";
        await channel.SendMessage(
            new SendableEmbed()
            {
                Title = "Content Detection - State Updated",
                Description = "Your server is eligible for Content Detection and it has been enabled!"
            });
        await controller.Set(d);
        return "Success";
    }

    private async Task<string?> DenyServer(string serverId, string reason = "")
    {
        var controller = Reflection.FetchModule<ContentDetectionServerConfigController>();
        var d = await controller.Fetch(serverId);
        if (d == null)
            return "ServerNotFound";

        d.Enabled = false;
        d.AllowAnalysis = false;
        d.HasRequested = false;
        var channel = await Client.GetChannel(d.LogChannelId) as TextChannel;
        if (channel == null)
            return "ChannelNotFound";
        await channel.SendMessage(
            new SendableEmbed()
            {
                Title = "Content Detection - State Updated",
                Description = $"Content Detection has been disabled for this server.\nReason\n```\n{reason}\n```"
            });
        await controller.Set(d);
        return "Success";
    }

    private async Task<string?> BanServer(string serverId, string reason = "")
    {
        var controller = Reflection.FetchModule<ContentDetectionServerConfigController>();
        var d = await controller.Fetch(serverId);
        if (d == null)
            return "ServerNotFound";

        d.Enabled = false;
        d.AllowAnalysis = false;
        d.IsBanned = true;
        d.BanReason = reason;
        var channel = await Client.GetChannel(d.LogChannelId) as TextChannel;
        if (channel == null)
            return "ChannelNotFound";
        await channel.SendMessage(
            new SendableEmbed()
            {
                Title = "Content Detection - State Updated",
                Description = $"Content Detection has banned for this server.\nReason\n```\n{reason}\n```"
            });
        await controller.Set(d);
        return "Success";
    }
}