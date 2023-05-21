using System.Text.Json.Serialization;

namespace RevSharp.Core.Models.WebSocket;

public class RoleIdEvent : IdEvent
{
    [JsonPropertyName("role_id")]
    public string RoleId { get; set; }
}