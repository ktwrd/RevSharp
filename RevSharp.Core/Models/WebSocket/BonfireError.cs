using System.Text.Json.Serialization;

namespace RevSharp.Core.Models.WebSocket;

public class BonfireError : BaseTypedResponse
{
    [JsonPropertyName("error")]
    public string Error { get; set; }
}