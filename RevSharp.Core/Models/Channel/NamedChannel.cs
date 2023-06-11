using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class NamedChannel : BaseChannel, INamedChannel
{
    /// <summary>
    /// Display name of the channel
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
    /// <summary>
    /// Channel description
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    /// <summary>
    /// Custom icon attachment
    /// </summary>
    [JsonPropertyName("icon")]
    public File? Icon { get; set; }
    
    #region Constructors
    public NamedChannel()
        : base(null, "")
    {}
    public NamedChannel(string id)
        : base(null,"")
    {}
    internal NamedChannel(Client? client, string id)
        : base(client, id)
    {}
    #endregion
}