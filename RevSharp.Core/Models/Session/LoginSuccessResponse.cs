using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class LoginSuccessResponse : LoginBaseResponse
{
    /// <summary>
    /// Unique Id
    /// </summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    /// <summary>
    /// User Id
    /// </summary>
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
    /// <summary>
    /// Session token
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; set; }
    /// <summary>
    /// Display name
    /// </summary>
    [JsonPropertyName("name")]
    public string DisplayName { get; set; }
    /// <summary>
    /// Web Push subscription
    /// </summary>
    [JsonPropertyName("subscription")]
    public WebPushSubscription? Subscription { get; set; }
}