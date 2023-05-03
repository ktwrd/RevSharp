using System.Text.Json.Serialization;

namespace RevSharp.Core.Models.WebSocket;

public class MessageReactedEvent : BaseTypedResponse
{
    [JsonPropertyName("id")]
    public string MessageId { get; set; }
    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; }
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
    [JsonPropertyName("emoji_id")]
    public string Emoji { get; set; }
}