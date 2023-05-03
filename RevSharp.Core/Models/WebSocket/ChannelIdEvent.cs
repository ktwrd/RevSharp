using System.Text.Json.Serialization;

namespace RevSharp.Core.Models.WebSocket;

public class ChannelIdEvent : BaseTypedResponse
{
    [JsonPropertyName("channel")]
    public string Channel { get; set; }

    protected ChannelIdEvent(string type, string channel)
    {
        Type = type;
        Channel = channel;
    }
    public ChannelIdEvent()
    {}
}