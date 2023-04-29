using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class FileMetadata : IFileMetadata
{
    /// <inheritdoc />
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("width")]
    public int? Width { get; set; }
    [JsonPropertyName("height")]
    public int? Height { get; set; }
}