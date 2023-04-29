namespace RevSharp.Core.Models;

public interface IFile<IFM> : ISnowflake where IFM : IFileMetadata
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
    IFM Metadata { get; set; }
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