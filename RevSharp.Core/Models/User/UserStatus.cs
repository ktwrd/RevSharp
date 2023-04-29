using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class UserStatus : IUserStatus
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
    [JsonPropertyName("presence")]
    public UserPresence Presence { get; set; }
}