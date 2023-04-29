namespace RevSharp.Core.Models;

public interface IFileMetadata
{
    /// <summary>
    /// Valid Values: File|Text|Image|Video|Audio
    /// </summary>
    string Type { get; set; }
    int? Width { get; set; }
    int? Height { get; set; }
}