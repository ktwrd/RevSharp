using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class PartialRole
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("permissions")]
    public PermissionCompare? Permissions { get; set; }
    [JsonPropertyName("colour")]
    public string? Colour { get; set; }
    [JsonPropertyName("hoist")]
    public bool? Hoist { get; set; }
    [JsonPropertyName("rank")]
    public long? Rank { get; set; }
}