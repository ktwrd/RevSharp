using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class ServerCategory
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("channels")]
    public string[] ChannelIds { get; set; }
}