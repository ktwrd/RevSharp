using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class UserUpdateData
{
    /// <summary>
    /// New display name
    /// </summary>
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
    /// <summary>
    /// Attachment Id for avatar
    /// </summary>
    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }
    
    [JsonPropertyName("status")]
    public UserUpdateStatusData? Status { get; set; }
    [JsonPropertyName("profile")]
    public UserUpdateProfileData? Profile { get; set; }
    /// <summary>
    /// Bitfield of user badges
    /// </summary>
    [JsonPropertyName("badges")]
    public int? Badges { get; set; }
    /// <summary>
    /// Enum of user flags
    /// </summary>
    [JsonPropertyName("flags")]
    public int? Flags { get; set; }
    [JsonPropertyName("remove")]
    public string[]? RemoveFields { get; set; }
}