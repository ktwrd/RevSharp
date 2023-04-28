using System.Text.Json.Serialization;

namespace RevSharp.Core.Models.WebSocket;

public class BonfireGenericData<T> : BaseTypedResponse
{
    [JsonPropertyName("data")]
    public T Data { get; set; }
}