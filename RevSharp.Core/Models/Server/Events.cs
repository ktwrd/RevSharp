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
}