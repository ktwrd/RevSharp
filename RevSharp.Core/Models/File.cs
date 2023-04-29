using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class File : /*IFile,*/ ISnowflake
{
    /// <inheritdoc />
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    /// <inheritdoc />
    [JsonPropertyName("tag")]
    public string Tag { get; set; }
    /// <inheritdoc />
    [JsonPropertyName("filename")]
    public string Filename { get; set; }
    /// <inheritdoc />
    [JsonPropertyName("metadata")]
    public FileMetadata Metadata { get; set; }
    /// <inheritdoc />
    [JsonPropertyName("content_type")]
    public string ContentType { get; set; }
    /// <inheritdoc />
    [JsonPropertyName("size")]
    public int Size { get; set; }
    /// <inheritdoc />
    [JsonPropertyName("deleted")]
    public bool? IsDeleted { get; set; }
    /// <inheritdoc />
    [JsonPropertyName("reported")]
    public bool? IsReported { get; set; }
    /// <inheritdoc />
    [JsonPropertyName("message_id")]
    public string? MessageId { get; set; }
    /// <inheritdoc />
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }
    /// <inheritdoc />
    [JsonPropertyName("server_id")]
    public string? ServerId { get; set; }
    /// <inheritdoc />
    [JsonPropertyName("object_id")]
    public string? ObjectId { get; set; }
}