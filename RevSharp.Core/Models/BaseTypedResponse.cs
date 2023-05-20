using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

/// <summary>
/// Base type for all Bonfire messages that are received.
/// </summary>
public class BaseTypedResponse : IBaseTypedResponse
{
    /// <summary>
    /// Type of the event
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }
}

public interface IBaseTypedResponse
{
    public string Type { get; set; }
}