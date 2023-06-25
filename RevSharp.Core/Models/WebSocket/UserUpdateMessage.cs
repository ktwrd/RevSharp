using System.Text.Json.Serialization;

namespace RevSharp.Core.Models.WebSocket;

public class UserUpdateMessage : BonfireGenericData<PartialUser>
{
    [JsonPropertyName("clear")]
    public string[]? Clear { get; set; }
    [JsonPropertyName("id")]
    public string UserId { get; set; }
}