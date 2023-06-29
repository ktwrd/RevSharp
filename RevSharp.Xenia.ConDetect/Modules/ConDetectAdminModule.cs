using System.Text.Json;
using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Modules;

[RevSharpModule]
public class ConDetectAdminModule : CommandModule
{
    public override async Task CommandReceived(CommandInfo info, Message message)
    {
        string action = "";
        if (info.Arguments.Count > 0)
            action = info.Arguments[0];
        switch (action)
        {
            case "help":
                await message.Reply(new SendableEmbed()
                {
                    Title = "Content Detection Admin - Help",
                    Description = HelpContent()
                });
                break;
            case "allow":
                await Command_Allow(info, message);
                break;
            case "deny":
                await Command_Deny(info, message);
                break;
            case "ban":
                await Command_Ban(info, message);
                break;
            case "dump":
                await Command_Dump(info, message);
                break;
            default:
                await message.Reply($"Unknown action `{action,-1}`");
                break;
        }
    }

    public async Task Command_Dump(CommandInfo info, Message message)
    {
        string targetId = "";
        if (info.Arguments.Count > 1)
            targetId = info.Arguments[1].ToUpper();
        
        var controller = Reflection.FetchModule<ContentDetectionServerConfigController>();
        var d = await controller.Fetch(targetId);
        if (d == null)
        {
            await message.Reply($"Server `{targetId,-1}` not found in database");
            return;
        }

        var stringContent = JsonSerializer.Serialize(d, Client.SerialzerOptions);
        await message.Reply($"```json\n{stringContent}\n```");
    }

    public async Task Command_Ban(CommandInfo info, Message message)
    {
        string targetId = "";
        if (info.Arguments.Count > 1)
            targetId = info.Arguments[1].ToUpper();
        string reason = "<none>";
        if (info.Arguments.Count > 2)
        {
            var theRest = info.Arguments.ToList();
            theRest.RemoveRange(0, 2);
            reason = string.Join(' ', theRest);
        }
        var banRes = await BanServer(targetId, reason);
        await message.Reply(new SendableEmbed()
        {
            Title = "Content Detection Admin - Update State",
            Description = $"State attempt set to banned. Result: `{banRes}`"
        });
    }
    public async Task Command_Deny(CommandInfo info, Message message)
    {
        string targetId = "";
        if (info.Arguments.Count > 1)
            targetId = info.Arguments[1].ToUpper();
        var allowRes = await DenyServer(targetId);
        await message.Reply(new SendableEmbed()
        {
            Title = "Content Detection Admin - Update State",
            Description = $"State attempt set to denied. Result: `{allowRes}`"
        });
    }

    public async Task Command_Allow(CommandInfo info, Message message)
    {
        string targetId = "";
        if (info.Arguments.Count > 1)
            targetId = info.Arguments[1].ToUpper();
        var allowRes = await AllowServer(targetId);
        await message.Reply(new SendableEmbed()
        {
            Title = "Content Detection Admin - Update State",
            Description = $"State attempt set to allowed. Result: `{allowRes}`"
        });
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

    public override string? HelpContent()
    {
        return XeniaHelper.GenerateHelp(this, new List<(string, string)>()
        {
            ("allow <id>", "Allow server to use content detection"),
            ("deny <id>", "Deny server access, but it can request again"),
            ("ban <id> <...reason>", "Deny server access, can't request again"),
            ("dump <id>", "Fetch db record as JSON")
        });
    }
    public override bool OwnerOnly => true;
    public override bool HasHelpContent => true;
    public override string? BaseCommandName => "cdadmin";
    public override string? HelpCategory => "admin";
}