using System.Text.Json;
using RevSharp.Core.Models.WebSocket;

namespace RevSharp.Core;

internal partial class WebsocketClient
{
    private async Task ParseMessage_User(string content, string type)
    {
        switch (type)
        {
            case "UserUpdate":
                var userUpdateData = JsonSerializer.Deserialize<UserUpdateMessage>(content, Client.SerializerOptions);
                if (userUpdateData != null)
                {
                    _client.OnUserUpdate(userUpdateData);
                }
                break;
            case "UserRelationship":
                var userRelationshipData =
                    JsonSerializer.Deserialize<UserRelationshipEvent>(content, Client.SerializerOptions);
                if (userRelationshipData != null)
                {
                    _client.OnUserRelationship(userRelationshipData);
                }
                break;
        }
    }
    
}