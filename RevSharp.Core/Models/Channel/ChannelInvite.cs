using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class ChannelInvite
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    [JsonPropertyName("server")]
    public string? ServerId { get; set; }
    [JsonPropertyName("creator")]
    public string CreatorId { get; set; }
    [JsonPropertyName("channel")]
    public string ChannelId { get; set; }
}