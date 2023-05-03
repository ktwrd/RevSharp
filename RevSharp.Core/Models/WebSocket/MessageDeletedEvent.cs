using System.Text.Json.Serialization;

namespace RevSharp.Core.Models.WebSocket;

public class MessageDeletedEvent : BaseTypedResponse
{
    [JsonPropertyName("id")]
    public string MessageId { get; set; }
    [JsonPropertyName("channel")]
    public string ChannelId { get; set; }
}