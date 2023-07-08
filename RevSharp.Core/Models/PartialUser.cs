using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class PartialUser
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    [JsonPropertyName("username")]
    public string? Username { get; set; }
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
    [JsonPropertyName("discriminator")]
    public string? Discriminator { get; set; }
    [JsonPropertyName("avatar")]
    public File? Avatar { get; set; }
    [JsonPropertyName("relations")]
    public UserRelation[]? Relations { get; set; }
    [JsonPropertyName("badges")]
    public int? Badges { get; set; }
    [JsonPropertyName("status")]
    public UserStatus? Status { get; set; }
    [JsonPropertyName("profile")]
    public UserProfile? Profile { get; set; }
    [JsonPropertyName("flags")]
    public UserFlags? Flags { get; set; }
    [JsonPropertyName("privileged")]
    public bool? Privileged { get; set; }
    [JsonPropertyName("bot")]
    public UserBotDetails? Bot { get; set; }
    [JsonPropertyName("relationship")]
    public UserRelationship? Relationship { get; set; }
    [JsonPropertyName("online")]
    public bool? Online { get; set; }
}