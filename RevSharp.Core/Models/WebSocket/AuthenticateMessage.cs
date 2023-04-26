using System.Text.Json.Serialization;

namespace RevSharp.Core.Models.WebSocket;

public class AuthenticateMessage : BaseWebSocketMessage
{
    [JsonPropertyName("type")]
    public new string Type => "Authenticate";
    [JsonPropertyName("token")]
    public string Token { get; set; }
}