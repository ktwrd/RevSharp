using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class File : IFile<FileMetadata>, ISnowflake
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    /// <summary>
    /// Tag / bucket this file was uploaded to
    /// </summary>
    [JsonPropertyName("tag")]
    public string Tag { get; set; }
    /// <summary>
    /// Original filename
    /// </summary>
    [JsonPropertyName("filename")]
    public string Filename { get; set; }
    /// <summary>
    /// Parsed metadata of this file
    /// </summary>
    [JsonPropertyName("metadata")]
    public FileMetadata Metadata { get; set; }
    /// <summary>
    /// Raw content type of this file
    /// </summary>
    [JsonPropertyName("content_type")]
    public string ContentType { get; set; }
    /// <summary>
    /// Size of this file (in bytes)
    /// </summary>
    [JsonPropertyName("size")]
    public int Size { get; set; }
    
    /// <summary>
    /// Wether this file was deleted
    /// </summary>
    [JsonPropertyName("deleted")]
    public bool? IsDeleted { get; set; }
    /// <summary>
    /// Wether this file was reported
    /// </summary>
    [JsonPropertyName("reported")]
    public bool? IsReported { get; set; }
    
    [JsonPropertyName("message_id")]
    public string? MessageId { get; set; }
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }
    [JsonPropertyName("server_id")]
    public string? ServerId { get; set; }
    
    /// <summary>
    /// Id of the file object this file is associated with
    /// </summary>
    [JsonPropertyName("object_id")]
    public string? ObjectId { get; set; }

    public string? GetURL(Client? client)
    {
        if (client?.EndpointNodeInfo == null)
            return null;

        var b = client.EndpointNodeInfo.Features.Autumn.Url;
        return $"{b}/{Tag}/{Id}/{Filename}";
    }
}