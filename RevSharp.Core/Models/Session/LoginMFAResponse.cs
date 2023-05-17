using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class LoginMFAResponse : LoginBaseResponse
{
    [JsonPropertyName("ticket")]
    public string Ticket { get; set; }
    [JsonPropertyName("allowed_methods")]
    public string[] AllowedMethods { get; set; }
}