using System.Text.Json.Serialization;

namespace RevSharp.Core.Models.WebSocket;

public class BonfireMessage : Message, IBaseTypedResponse
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
}