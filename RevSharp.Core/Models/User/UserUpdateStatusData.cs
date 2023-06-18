using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class UserUpdateStatusData
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
    [JsonPropertyName("presence")]
    public string? PresenceString { get; set; }
}