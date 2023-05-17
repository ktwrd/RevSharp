using System.Text.Json;
using RevSharp.Core.Models;

namespace RevSharp.Core;

internal partial class WebsocketClient
{
    private async Task ParseMessage_Server(string content, string type)
    {
        switch (type)
        {
            case "ServerCreate":
                var serverCreateData = JsonSerializer.Deserialize<Server>(content, Client.SerializerOptions);
                serverCreateData.Client = _client;
                await serverCreateData.Fetch();
                _client.AddToCache(serverCreateData);
                _client.OnServerCreate(_client.ServerCache[serverCreateData.Id]);
                break;
        }
    }
}