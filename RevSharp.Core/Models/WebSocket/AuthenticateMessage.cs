using System.Text.Json.Serialization;

namespace RevSharp.Core.Models.WebSocket;

public class AuthenticateMessage : BaseTypedResponse
{
    [JsonPropertyName("type")]
    public new string Type => "Authenticate";
    [JsonPropertyName("token")]
    public string Token { get; set; }
}