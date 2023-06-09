﻿using System.Text.Json;
using RevSharp.Core.Models;
using RevSharp.Core.Models.WebSocket;

namespace RevSharp.Core;

internal partial class WebsocketClient
{
    private async Task ParseMessage_Server(string content, string type)
    {
        if (type.StartsWith("ServerRole"))
        {
            await ParseMessage_ServerRole(content, type);
            return;
        }
        switch (type)
        {
            case "ServerCreate":
                var serverCreateData = JsonSerializer.Deserialize<Server>(
                    content,
                    Client.SerializerOptions);
                if (serverCreateData != null)
                {
                    _client.OnServerCreated(_client.ServerCache[serverCreateData.Id]);
                }
                break;
            case "ServerDelete":
                var serverDeleteData = JsonSerializer.Deserialize<IdEvent>(
                    content,
                    Client.SerializerOptions);
                if (serverDeleteData != null)
                {
                    _client.OnServerDeleted(serverDeleteData.Id);
                }
                break;
            case "ServerMemberJoin":
                var serverMemberJoinData = JsonSerializer.Deserialize<UserIdEvent>(
                    content,
                    Client.SerializerOptions);
                if (serverMemberJoinData != null)
                {
                    _client.OnServerMemberJoined(serverMemberJoinData.Id, serverMemberJoinData.UserId);
                }
                break;
            case "ServerMemberLeave":
                var serverMemberLeaveData = JsonSerializer.Deserialize<UserIdEvent>(
                    content,
                    Client.SerializerOptions);
                if (serverMemberLeaveData != null)
                {
                    _client.OnServerMemberLeft(serverMemberLeaveData.Id, serverMemberLeaveData.UserId);
                }
                break;
        }
    }
}