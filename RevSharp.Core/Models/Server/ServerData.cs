using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class ServerData
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }
    [JsonPropertyName("banner")]
    public string? Banner { get; set; }
    [JsonPropertyName("categories")]
    public ServerCategory[]? Categories { get; set; }
    [JsonPropertyName("flags")]
    public int? Flags { get; set; }
    [JsonPropertyName("discoverable")]
    public bool? Discoverable { get; set; }
    [JsonPropertyName("analytics")]
    public bool? Analytics { get; set; }
    [JsonPropertyName("remove")]
    public string[]? Remove { get; set; }
}