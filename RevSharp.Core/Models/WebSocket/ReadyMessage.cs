using System.Text.Json.Serialization;

namespace RevSharp.Core.Models.WebSocket;

public class ReadyMessage : BaseTypedResponse
{
    [JsonPropertyName("type")]
    public new string Type => "Ready";
    [JsonPropertyName("users")]
    public User[] Users { get; set; }
    [JsonPropertyName("servers")]
    public Server[] Servers { get; set; }
    [JsonPropertyName("channels")]
    public BaseChannel[] Channels { get; set; }
    [JsonPropertyName("emojis")]
    public Emoji[]? Emojis { get; set; }
}