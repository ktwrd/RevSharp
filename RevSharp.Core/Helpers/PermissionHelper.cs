using System.Diagnostics;
using RevSharp.Core.Models;

namespace RevSharp.Core.Helpers;

/// <summary>
/// all of this is ported from revolt.js/src/permissions/calculator.ts
/// </summary>
public static class PermissionHelper
{
    public const long DEFAULT_PERMISSION_DIRECT_MESSAGE =
        DEFAULT_PERMISSION +
        (long)PermissionFlag.React +
        (long)PermissionFlag.ManageChannel;

    public const long DEFAULT_PERMISSION =
        DEFAULT_PERMISSION_VIEW_ONLY +
        (long)PermissionFlag.SendMessage +
        (long)PermissionFlag.InviteOthers +
        (long)PermissionFlag.SendEmbeds +
        (long)PermissionFlag.UploadFiles +
        (long)PermissionFlag.Connect +
        (long)PermissionFlag.Speak;

    public const long DEFAULT_PERMISSION_VIEW_ONLY =
        (long)PermissionFlag.ViewChannel +
        (long)PermissionFlag.ReadMessageHistory;

    public const long ALLOW_IN_TIMEOUT =
        (long)PermissionFlag.ViewChannel +
        (long)PermissionFlag.ReadMessageHistory;
    public static async Task<long> CalculatePermission(Client client, User user, BaseChannel channel)
    {
        if (user.IsPrivileged)
            return (long)PermissionFlag.GrantAllSafe;

        switch (channel.ChannelType)
        {
            case "SavedMessages":
                return (long)PermissionFlag.GrantAllSafe;
                break;
            case "DirectMessage":
                var dmChannel = (DirectMessageChannel)channel;
                var dmChannelRecipient = await dmChannel.FetchRecipient(client, forceUpdate: false);
                var dmChannelRecipientPermission = dmChannelRecipient?.Permission ?? 0;

                if ((dmChannelRecipientPermission & (long)UserPermission.SendMessage) == 1)
                {
                    return DEFAULT_PERMISSION_DIRECT_MESSAGE;
                }
                else
                {
                    return DEFAULT_PERMISSION_VIEW_ONLY;
                }
                break;
            case "Group":
                var groupChannel = (GroupChannel)channel;
                if (groupChannel.OwnerId == user.Id)
                {
                    return DEFAULT_PERMISSION_DIRECT_MESSAGE;
                }
                else
                {
                    return groupChannel.Permissions ?? DEFAULT_PERMISSION_DIRECT_MESSAGE;
                }
                break;
            case "TextChannel":
            case "VoiceChannel":
                var textChannel = (TextChannel)channel;
                var server = await client.GetServer(textChannel.ServerId, forceUpdate: false);
                if (server == null)
                    return 0;
                if (server.OwnerId == user.Id)
                    return (long)PermissionFlag.GrantAllSafe;

                var member = server.Members.Where(v => v.Id.UserId == user.Id).FirstOrDefault();
                if (member == null)
                    return 0;

                var perm = await CalculatePermission(client, user, server);
                if (server.DefaultPermissions != null)
                    perm = perm
                           | textChannel.DefaultPermissions.Allow
                           & (~textChannel.DefaultPermissions.Deny);

                if (member.RoleIds.Length > 0 && textChannel.RolePermissions.Count > 0 && server.Roles.Count > 0)
                {
                    var roles = await member.FetchOrderedRoles(client);
                    foreach (var thing in roles)
                    {
                        if (textChannel.RolePermissions.TryGetValue(thing.Item1, out var overide))
                        {
                            perm = perm | overide.Allow & (~overide.Deny);
                        }
                    }
                }

                if (member.TimeoutTimestamp != null && member.TimeoutTimestamp > DateTimeOffset.UtcNow)
                {
                    perm = perm & ALLOW_IN_TIMEOUT;
                }

                return perm;
                break;
            default:
                return 0;
        }
    }

    public static async Task<long> CalculatePermission(Client client, User user, Server server)
    {
        if (server.OwnerId == user.Id)
            return (long)PermissionFlag.GrantAllSafe;

        var member = await server.GetMember(user.Id);
        if (member == null)
            return 0;
        var perm = server.DefaultPermissions;
        var memberRoles = await member.FetchOrderedRoles(client, forceUpdate: false);
        if (memberRoles is { Count: > 0 } && server.Roles.Count > 0)
        {
            var permissions = memberRoles.Select(v => v.Item2.Permissions ?? new PermissionCompare()
            {
                Allow = 0,
                Deny = 0
            });
            foreach (var p in permissions)
            {
                perm = perm | p.Allow & (~p.Deny);
            }
        }

        if (member.TimeoutTimestamp != null && member.TimeoutTimestamp > DateTimeOffset.UtcNow)
            perm = perm & ALLOW_IN_TIMEOUT;
        
        return perm;
    }
    public static PermissionFlag[] GetPermissions(long allow, long deny)
    {
        var flags = new List<PermissionFlag>();
        foreach (var item in Enum.GetValues(typeof(PermissionFlag)).Cast<PermissionFlag>())
        {
            var longItem = (int)item;
            var value = GetValue(allow, deny, longItem);
            if (value == PermissionValue.Allow)
            {
                Console.WriteLine($"{item,30}");
                flags.Add(item);
            }
        }
        return flags.ToArray();
    }

    public static PermissionValue GetValue(long allow, long deny, long flag)
    {
        if (HasFlag(allow, flag))
            return PermissionValue.Allow;
        else if (HasFlag(deny, flag))
            return PermissionValue.Deny;
        else
            return PermissionValue.Inherit;
    }

    public enum PermissionValue
    {
        Allow,
        Deny,
        Inherit
    }

    public static bool HasFlag(long value, long flag)
        => (value & flag) == flag;
}