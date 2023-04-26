using System.Text.Json.Serialization;

namespace RevSharp.Core.Models.WebSocket;

public class AuthenticateMessage : BaseWebSocketMessage
{
    public AuthenticateMessage()
    {
        Type = "Authenticate";
    }
    [JsonPropertyName("token")]
    public string Token { get; set; }
}