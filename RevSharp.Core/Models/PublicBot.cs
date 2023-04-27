using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class PublicBot : ISnowflake
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    [JsonPropertyName("username")]
    public string Username { get; set; }
    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
}