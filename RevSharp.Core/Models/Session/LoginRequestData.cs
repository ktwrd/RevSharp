using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class LoginRequestData
{
    [JsonPropertyName("email")]
    public string Email { get; set; }
    [JsonPropertyName("password")]
    public string Password { get; set; }
    [JsonPropertyName("friendly_name")]
    public string? FriendlyName { get; set; }
}