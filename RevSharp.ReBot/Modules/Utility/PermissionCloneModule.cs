using RevSharp.Core.Models;
using RevSharp.ReBot.Helpers;
using RevSharp.ReBot.Reflection;

namespace RevSharp.ReBot.Modules.Utility;

[RevSharpModule]
public class PermissionCloneModule : BaseModule
{
    public override string? HelpCategory => "utility";
    public override string? InternalName => "permissionclone";
    public override bool HasHelpContent => true;

    public override string HelpContent()
    {
        return string.Join(
            "\n", new string[]
            {
                "```",
                "r.permissionclone channel <source channel> ...<target channels>"
            });
    }

    private async Task Command_PermissionCloneChannel(CommandInfo info, Message message)
    {
        string sourceChannelId = "";
        var targetChannelIds = new List<string>();
        for (int i = 0; i < info.Arguments.Count; i++)
        {
            var arg = info.Arguments[i].ToLower();
            if (i == 1)
            {
                if (arg == "current" || arg == "this")
                {
                    sourceChannelId = message.ChannelId;
                }
                else
                {
                    var found = CommandHelper.FindChannelId(arg.ToUpper());
                    if (found != null)
                        sourceChannelId = found;
                }
            }

            if (i > 1)
            {
                var found = CommandHelper.FindChannelId(arg.ToUpper());
                if (found != null)
                    targetChannelIds.Add(found);
            }
        }

        var embed = new SendableEmbed()
        {
            Title = "Permission Cloning"
        };
        if (sourceChannelId.Length < 1)
        {
            embed.Description = string.Join(
                "\n", new string[]
                {
                    "Invalid/Missing source channel.",
                    "",
                    "Valid Values;",
                    "```",
                    "current      - Current channel",
                    "this         - Current channel",
                    "<#XXXXX>     - Mentioned channel",
                    "XXXXXX       - Channel Id",
                    "```"
                });
            await message.Reply(embed);
            return;
        }

        if (targetChannelIds.Count < 1)
        {
            embed.Description = string.Join(
                "\n", new string[]
                {
                    "No target channels provided. Everything after the source channel is a target channel.",
                    "It can be the Channel Id directly, or the mentioned channel."
                });
            await message.Reply(embed);
            return;
        }

        var sourceChannel = await Client.GetChannel(sourceChannelId) as VoiceChannel;
        int failCount = 0;
        int totalCount = 0;
        foreach (var channelId in targetChannelIds)
        {
            var targetChannel = await Client.GetChannel(channelId) as VoiceChannel;
            if (targetChannel == null)
            {
                failCount++;
                continue;
            }

            foreach (var sourcePair in sourceChannel.RolePermissions)
            {
                if (!targetChannel.RolePermissions.ContainsKey(sourcePair.Key) ||
                    (targetChannel.RolePermissions.TryGetValue(sourcePair.Key, out PermissionCompare value) &&
                     value != sourcePair.Value))
                {
                    failCount += await targetChannel.SetRolePermission(sourcePair.Key, sourcePair.Value.Allow, sourcePair.Value.Deny) ?
                        0 :
                        1;
                    totalCount++;
                }
            }

            if (sourceChannel.DefaultPermissions != null &&
                targetChannel.DefaultPermissions != null &&
                targetChannel.DefaultPermissions.Allow != sourceChannel.DefaultPermissions.Allow &&
                targetChannel.DefaultPermissions.Deny != sourceChannel.DefaultPermissions.Deny)
            {
                failCount += await targetChannel.SetDefaultPermission(
                    sourceChannel.DefaultPermissions.Allow,
                    sourceChannel.DefaultPermissions.Deny) ?
                    0 :
                    1;
                totalCount++;
            }
        }

        embed.Description = string.Join(
            "\n", new string[]
            {
                $"Processed {targetChannelIds.Count} channels ({failCount}/{totalCount} failures)"
            });
        await message.Reply(embed);
    }

    private async Task Command_Help(CommandInfo info, Message message)
    {
        var embed = new SendableEmbed()
        {
            Title = "Permission Cloning - Help"
        };
        embed.Description = HelpContent();
        await message.Reply(embed);
    }

    public override async Task MessageReceived(Message message)
    {
        var info = CommandHelper.FetchInfo(message);
        if (info?.Command != InternalName)
            return;

        var action = "help";
        if (info.Arguments.Count > 0)
            action = info.Arguments[0].ToLower();
        switch (action)
        {
            case "channel":
                await Command_PermissionCloneChannel(info, message);
                break;
            default:
                await Command_Help(info, message);
                break;
            case "help":
                await Command_Help(info, message);
                break;
        }
    }
}