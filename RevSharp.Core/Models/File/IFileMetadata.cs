namespace RevSharp.Core.Models;

public interface IFileMetadata
{
    /// <summary>
    /// Valid Values: File|Text|Image|Video|Audio
    /// </summary>
    public string Type { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
}