using System.Net;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class Bot : ISnowflake
{
    /// <summary>
    /// Bot Id
    ///
    /// This equals the associated bot user's id.
    /// </summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    /// <summary>
    /// User Id of the bot owner
    /// </summary>
    [JsonPropertyName("owner")]
    public string OwnerId { get; set; }
    /// <summary>
    /// Token used to authenticate requests for this bot
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; set; }
    /// <summary>
    /// Whether the bot is public
    /// (may be invited by anyone)
    /// </summary>
    [JsonPropertyName("public")]
    public bool IsPublic { get; set; }

    /// <summary>
    /// Whether to enable analytics
    /// </summary>
    [JsonPropertyName("analytics")]
    public bool EnableAnalytics { get; set; }
    /// <summary>
    /// Whether this bot should be publicly discoverable
    /// </summary>
    [JsonPropertyName("discoverable")]
    public bool IsDiscoverable { get; set; }
    /// <summary>
    /// Reserved; URL for handling interactions
    /// </summary>
    [JsonPropertyName("interactions_url")]
    public string? InteractionsUrl { get; set; }
    /// <summary>
    /// URL for terms of service
    /// </summary>
    [JsonPropertyName("terms_of_service_url")]
    public string? TermsOfServiceUrl { get; set; }
    /// <summary>
    /// URL for privacy policy
    /// </summary>
    [JsonPropertyName("privacy_policy_url")]
    public string? PrivacyPolicyUrl { get; set; }
    
    /// <summary>
    /// Enum of bot flags
    /// </summary>
    [JsonPropertyName("flags")]
    public BotFlags? Flags { get; set; }

    public async Task<bool> Delete(Client client)
    {
        var response = await client.DeleteAsync($"/bots/{Id}");
        return response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent;
    }
}

public enum BotFlags : int
{
    Verified = 1,
    Official = 2
}