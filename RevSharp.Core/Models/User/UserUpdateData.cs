using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class UserUpdateData
{
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }
    [JsonPropertyName("status")]
    public UserUpdateStatusData? Status { get; set; }
    [JsonPropertyName("profile")]
    public UserUpdateProfileData? Profile { get; set; }
    [JsonPropertyName("badges")]
    public int? Badges { get; set; }
    [JsonPropertyName("flags")]
    public int? Flags { get; set; }
    [JsonPropertyName("remove")]
    public string[]? RemoveFields { get; set; }
}