using kate.shared.Helpers;
using RevSharp.Core.Helpers;

namespace RevSharp.Core.Models;

public partial class Server
{
    public event VoidDelegate Deleted;
    /// <summary>
    /// Invoke <see cref="Deleted"/>
    /// </summary>
    internal void OnDeleted()
    {
        Deleted?.Invoke();
    }

    public event UserIdDelegate MemberJoined;
    /// <summary>
    /// Invoke <see cref="MemberJoined"/>
    /// </summary>
    internal void OnMemberJoined(string userId)
    {
        MemberJoined?.Invoke(userId);
    }

    public event UserIdDelegate MemberLeft;
    /// <summary>
    /// - If exists in <see cref="MemberCache"/>
    ///     - Invoke <see cref="Member.OnLeft()"/>
    /// - Remove userId from <see cref="MemberCache"/>
    /// - Invoke <see cref="MemberLeft"/>
    /// </summary>
    /// <param name="userId"></param>
    internal void OnMemberLeft(string userId)
    {
        if (MemberCache.TryGetValue(userId, out var value))
            value.OnLeft();
        MemberCache.Remove(userId);
        MemberLeft?.Invoke(userId);
    }

    public event RoleIdDelegate RoleDeleted;

    /// <summary>
    /// - If exists in <see cref="Roles"/>
    ///     - Invoke <see cref="ServerRole.Deleted"/>
    ///     - Remove from <see cref="Roles"/>
    /// - For ever member
    ///     - If has roleId force update calculated permissions
    ///     - If has roleId then remove it from that member
    /// - Invoke <see cref="RoleDeleted"/>
    /// </summary>
    internal void OnRoleDeleted(string roleId)
    {
        if (Roles.ContainsKey(roleId))
        {
            Roles[roleId].OnDeleted();
            Roles.Remove(roleId);
        }

        foreach (var item in Members)
        {
            if (item.RoleIds.Contains(roleId))
            {
                // Remove roleId from member
                item.RoleIds = item.RoleIds
                    .Where(v => v != roleId)
                    .ToArray();
                
                // Force update permission for this member
                item.HasPermission(
                    PermissionFlag.ManageChannel,
                    forceUpdate: true);
            }
        }

        RoleDeleted?.Invoke(roleId);
    }
}