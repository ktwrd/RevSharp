using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

/// <summary>
/// Information about what owns this emoji
/// </summary>
public class EmojiParent
{
    [JsonPropertyName("name")]
    public string Type { get; set; }
    /// <summary>
    /// Only set when <see cref="Type"/> is `Server`
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}