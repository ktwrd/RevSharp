using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class Masquerade
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }
    [JsonPropertyName("colour")]
    public string Colour { get; set; }
}