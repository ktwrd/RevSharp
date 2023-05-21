using System.Text.Json.Serialization;

namespace RevSharp.Core.Models.WebSocket;

public class ChannelTypingEvent : BaseTypedResponse
{
    [JsonPropertyName("id")]
    public string ChannelId { get; set; }
    [JsonPropertyName("user")]
    public string UserId { get; set; }
}