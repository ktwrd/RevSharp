using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

/// <summary>
/// Public Bot
/// </summary>
public class PublicBot : ISnowflake
{
    /// <summary>
    /// Bot Id
    /// </summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    /// <summary>
    /// Bot Username
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; }
    /// <summary>
    /// Profile Avatar
    /// </summary>
    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }
    /// <summary>
    /// Profile Description
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }
}