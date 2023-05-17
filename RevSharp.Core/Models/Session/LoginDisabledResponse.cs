using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class LoginDisabledResponse : LoginBaseResponse
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
}