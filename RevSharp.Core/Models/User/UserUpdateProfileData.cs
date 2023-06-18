using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class UserUpdateProfileData
{
    /// <summary>
    /// Text to set as user profile description
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    /// <summary>
    /// Attachment Id for background
    /// </summary>
    [JsonPropertyName("background")]
    public string? Background { get; set; }
}