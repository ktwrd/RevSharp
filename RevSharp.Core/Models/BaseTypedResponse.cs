using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

/// <summary>
/// Base type for all Bonfire messages that are received.
/// </summary>
public class BaseTypedResponse : IBaseTypedResponse
{
    /// <inheritdoc />
    [JsonPropertyName("type")]
    public string Type { get; set; }
}

public interface IBaseTypedResponse
{
    /// <summary>
    /// Type of the event
    /// </summary>
    public string Type { get; set; }
}