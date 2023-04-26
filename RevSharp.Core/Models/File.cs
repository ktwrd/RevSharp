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

public interface IFile : ISnowflake
{
    /// <summary>
    /// Tag / bucket this file was uploaded to
    /// </summary>
    string Tag { get; set; }
    /// <summary>
    /// Original filename
    /// </summary>
    string Filename { get; set; }
    /// <summary>
    /// Parsed metadata of this file
    /// </summary>
    IFileMetadata Metadata { get; set; }
    /// <summary>
    /// Raw content type of this file
    /// </summary>
    string ContentType { get; set; }
    /// <summary>
    /// Size of this file (in bytes)
    /// </summary>
    int Size { get; set; }
    
    /// <summary>
    /// Wether this file was deleted
    /// </summary>
    bool? IsDeleted { get; set; }
    /// <summary>
    /// Wether this file was reported
    /// </summary>
    bool? IsReported { get; set; }
    
    
    string? MessageId { get; set; }
    string? UserId { get; set; }
    string? ServerId { get; set; }

    /// <summary>
    /// Id of the file object this file is associated with
    /// </summary>
    string? ObjectId { get; set; }
}

public interface IFileMetadata
{
    /// <summary>
    /// Valid Values: File|Text|Image|Video|Audio
    /// </summary>
    string Type { get; set; }
    int? Width { get; set; }
    int? Height { get; set; }
}