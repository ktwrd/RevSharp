using kate.shared.Helpers;
using RevSharp.Core.Helpers;

namespace RevSharp.Core.Models;

public partial class Server
{
    public event VoidDelegate ServerDeleted;

    internal void OnServerDeleted()
    {
        ServerDeleted?.Invoke();
    }

    public event UserIdDelegate MemberJoined;

    internal void OnMemberJoined(string userId)
    {
        MemberJoined?.Invoke(userId);
    }
}