using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class BaseTypedResponse : IBaseTypedResponse
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
}

public interface IBaseTypedResponse
{
    public string Type { get; set; }
}