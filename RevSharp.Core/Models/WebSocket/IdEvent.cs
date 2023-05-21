using System.Text.Json.Serialization;

namespace RevSharp.Core.Models.WebSocket;

public class IdEvent : BaseTypedResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
}