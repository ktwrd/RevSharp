using System.Text.Json.Serialization;

namespace RevSharp.Core.Models.Errors;

public class RevoltHTTPErrorResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }
    [JsonPropertyName("reason")]
    public string Reason { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
}

public class RevoltGenericErrorResponse<T>
{
    [JsonPropertyName("error")]
    public T Error { get; set; }
}