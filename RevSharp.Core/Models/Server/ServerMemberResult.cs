using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class ServerMemberResult
{
    [JsonPropertyName("members")]
    public Member[] Members { get; set; }
    [JsonPropertyName("users")]
    public User[] Users { get; set; }
}