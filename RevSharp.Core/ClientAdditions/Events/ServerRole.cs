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

    /// <summary>
    /// Emitted when a server role has been updated.
    /// </summary>
    public event ServerRoleDelegate ServerRoleUpdate;

    /// <summary>
    /// - When Role exists
    ///     - Invoke <see cref="ServerRole.OnUpdate()"/>
    /// - Invoke <see cref="ServerRoleUpdate"/>
    /// </summary>
    /// <param name="message">Websocket message</param>
    internal void OnServerRoleUpdate(ServerRoleUpdateMessage message)
    {
        foreach (var server in ServerCache)
        {
            if (server.Value.Roles.TryGetValue(message.RoleId, out var role))
            {
                server.Value.OnRoleUpdate(message);
                ServerRoleUpdate?.Invoke(server.Key, role);
            }
        }
    }
}