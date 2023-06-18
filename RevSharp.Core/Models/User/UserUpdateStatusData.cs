using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class UserUpdateStatusData
{
    /// <summary>
    /// Custom status text
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }
    /// <summary>
    /// Current presence option
    /// </summary>
    [JsonPropertyName("presence")]
    public string? PresenceString { get; set; }
}