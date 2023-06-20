using System.ComponentModel;
using System.Net;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

/// <summary>
/// Bot account type. Used when fetching information about a bot.
/// </summary>
public class Bot : IBot
{
    /// <summary>
    /// Bot Id
    ///
    /// This equals the associated bot user's id.
    /// </summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    /// <inheritdoc />
    [JsonPropertyName("owner")]
    public string OwnerId { get; set; }
    /// <inheritdoc />
    [JsonPropertyName("token")]
    public string Token { get; set; }
    /// <inheritdoc />
    [JsonPropertyName("public")]
    public bool IsPublic { get; set; }

    /// <inheritdoc />
    [JsonPropertyName("analytics")]
    public bool EnableAnalytics { get; set; }
    /// <inheritdoc />
    [JsonPropertyName("discoverable")]
    public bool IsDiscoverable { get; set; }
    /// <inheritdoc />
    [JsonPropertyName("interactions_url")]
    public string? InteractionsUrl { get; set; }
    /// <inheritdoc />
    [JsonPropertyName("terms_of_service_url")]
    public string? TermsOfServiceUrl { get; set; }
    /// <inheritdoc />
    [JsonPropertyName("privacy_policy_url")]
    public string? PrivacyPolicyUrl { get; set; }
    
    /// <inheritdoc />
    [JsonPropertyName("flags")]
    public BotFlags? Flags { get; set; }

    /// <inheritdoc />
    public async Task<bool> Delete(Client client)
    {
        var response = await client.DeleteAsync($"/bots/{Id}");
        return response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent;
    }
}

/// <summary>
/// Flags for <see cref="IBot"/>
/// </summary>
[DefaultValue(BotFlags.Unknown)]
public enum BotFlags
{
    /// <summary>
    /// Failed to deserialize flag (default value)
    /// </summary>
    Unknown,
    /// <summary>
    /// Bot is verified by Revolt
    /// </summary>
    Verified = 1,
    /// <summary>
    /// 1st Party Revolt Bot
    /// </summary>
    Official = 2
}