using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class ServerRole
{
    /// <summary>
    /// Role Name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
    /// <summary>
    /// This can be any valid CSS colour
    /// </summary>
    [JsonPropertyName("colour")]
    public string? Colour { get; set; }
    /// <summary>
    /// Whether this role should be shown separately on the member sidebar
    /// </summary>
    [JsonPropertyName("hoist")]
    public bool Hoist { get; set; }
    /// <summary>
    /// Ranking of this role
    /// </summary>
    [JsonPropertyName("rank")]
    public long Rank { get; set; }
    [JsonPropertyName("permissions")]
    public PermissionCompare Permissions { get; set; }
}