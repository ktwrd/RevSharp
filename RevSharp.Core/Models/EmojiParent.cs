using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class EmojiParent
{
    [JsonPropertyName("name")]
    public string Type { get; set; }
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}