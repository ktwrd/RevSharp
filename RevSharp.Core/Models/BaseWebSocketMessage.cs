using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class BaseWebSocketMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
}