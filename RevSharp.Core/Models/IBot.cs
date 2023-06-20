namespace RevSharp.Core.Models;

public interface IBot : ISnowflake
{
    /// <summary>
    /// User Id of the bot owner
    /// </summary>
    public string OwnerId { get; set; }
    /// <summary>
    /// Token used to authenticate requests for this bot
    /// </summary>
    public string Token { get; set; }
    /// <summary>
    /// Whether the bot is public
    /// (may be invited by anyone)
    /// </summary>
    public bool IsPublic { get; set; }
    
    /// <summary>
    /// Whether to enable analytics
    /// </summary>
    public bool EnableAnalytics { get; set; }
    /// <summary>
    /// Whether this bot should be publicly discoverable
    /// </summary>
    public bool IsDiscoverable { get; set; }
    /// <summary>
    /// Reserved; URL for handling interactions
    /// </summary>
    public string? InteractionsUrl { get; set; }
    /// <summary>
    /// URL for terms of service
    /// </summary>
    public string? TermsOfServiceUrl { get; set; }
    /// <summary>
    /// URL for privacy policy
    /// </summary>
    public string? PrivacyPolicyUrl { get; set; }
    
    /// <summary>
    /// Enum of bot flags
    /// </summary>
    public BotFlags? Flags { get; set; }


    /// <summary>
    /// Delete this bot. Client must be the owner of this bot.
    /// </summary>
    /// <param name="client">Client to use when deleting this bot.</param>
    /// <returns>Was it successful? (Status Code is 2xx)</returns>
    public Task<bool> Delete(Client client);
}