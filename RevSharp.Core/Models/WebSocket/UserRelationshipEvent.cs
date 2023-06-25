using System.Text.Json.Serialization;

namespace RevSharp.Core.Models.WebSocket;

public class UserRelationshipEvent : BaseTypedResponse
{
    [JsonPropertyName("id")]
    public string UserId { get; set; }
    [JsonPropertyName("user")]
    public User UserData { get; set; }
    [JsonPropertyName("status")]
    public UserRelationship Status { get; set; }
}