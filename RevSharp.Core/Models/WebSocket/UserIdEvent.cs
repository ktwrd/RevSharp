using System.Text.Json.Serialization;

namespace RevSharp.Core.Models.WebSocket;

public class UserIdEvent : IdEvent
{
    [JsonPropertyName("user")]
    public string UserId {get;set;}
}