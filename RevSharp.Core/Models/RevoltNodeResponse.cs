using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class RevoltNodeResponse
{
    [JsonPropertyName("revolt")]
    public string Version { get; set; }
    [JsonPropertyName("features")]
    public RevoltNodeFeatures Features { get; set; }
    [JsonPropertyName("ws")]
    public string WebSocket { get; set; }
    [JsonPropertyName("app")]
    public string Frontend { get; set; }
    [JsonPropertyName("vapid")]
    public string VapId { get; set; }
}