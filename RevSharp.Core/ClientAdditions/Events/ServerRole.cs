using RevSharp.Core.Helpers;
using RevSharp.Core.Models;
using RevSharp.Core.Models.WebSocket;

namespace RevSharp.Core;

public partial class Client
{
    /// <summary>
    /// Emitted when a server role has been deleted
    /// </summary>
    public event ServerRoleIdDelegate ServerRoleDeleted;

    /// <summary>
    /// - When Server exists in cache
    ///     - Invoke <see cref="Server.OnRoleDeleted(string)"/>
    /// - Invoke <see cref="ServerRoleDeleted"/>
    /// </summary>
    /// <param name="serverId"></param>
    /// <param name="roleId"></param>
    internal void OnServerRoleDeleted(string serverId, string roleId)
    {
        if (ServerCache.ContainsKey(serverId))
        {
            ServerCache[serverId].OnRoleDeleted(roleId);
        }
        ServerRoleDeleted?.Invoke(serverId, roleId);
    }
    }
}