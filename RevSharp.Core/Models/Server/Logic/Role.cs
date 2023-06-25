using RevSharp.Core.Helpers;
using RevSharp.Core.Models.WebSocket;

namespace RevSharp.Core.Models;

public partial class Server
{
    /// <summary>
    /// Emitted when a server role has been updated
    /// </summary>
    public event ServerRoleDelegate RoleUpdate;

    internal void OnRoleUpdate(ServerRoleUpdateMessage message)
    {
        if (Roles.ContainsKey(message.RoleId))
        {
            ServerRole.Inject(message, Roles[message.RoleId]);
            Roles[message.RoleId].OnUpdate();
        }
    }
}