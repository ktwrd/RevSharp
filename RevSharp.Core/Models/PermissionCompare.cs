using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class PermissionCompare
{
    [JsonPropertyName("a")]
    public long Allow { get; set; }
    [JsonPropertyName("d")]
    public long Deny { get; set; }
}