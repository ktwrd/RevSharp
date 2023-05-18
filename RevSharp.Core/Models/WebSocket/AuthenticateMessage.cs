using System.Text.Json.Serialization;

namespace RevSharp.Core.Models.WebSocket;

public class AuthenticateMessage : BaseTypedResponse
{
    [JsonPropertyName("name")]
    public string? DisplayName { get; set; }
    [JsonPropertyName("token")]
    public string Token { get; set; }
    [JsonPropertyName("result")]
    public string? Result { get; set; }

    [JsonPropertyName("type")]
    public new string Type { get; set; } = "Authenticate";
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

}