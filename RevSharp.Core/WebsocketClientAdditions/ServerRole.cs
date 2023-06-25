using System.Text.Json;
using RevSharp.Core.Models.WebSocket;

namespace RevSharp.Core;

internal partial class WebsocketClient
{
    private async Task ParseMessage_ServerRole(string content, string type)
    {
        switch (type)
        {
            case "ServerRoleDelete":
                var serverRoleDeleteData = JsonSerializer.Deserialize<RoleIdEvent>(
                    content,
                    Client.SerializerOptions);
                if (serverRoleDeleteData != null)
                {
                    _client.OnServerRoleDeleted(serverRoleDeleteData.Id, serverRoleDeleteData.RoleId);
                }
                break;
            case "ServerRoleUpdate":
                var serverRoleUpdateData =
                    JsonSerializer.Deserialize<ServerRoleUpdateMessage>(content, Client.SerializerOptions);
                if (serverRoleUpdateData != null)
                {
                    _client.OnServerRoleUpdate(serverRoleUpdateData);
                }
                break;
        }
    }
}