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
}