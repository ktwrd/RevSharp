using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

/// <summary>
/// Direct message channel between two users
/// </summary>
public class DirectMessageChannel : BaseChannel, IFetchable
{
    /// <summary>
    /// Whether this direct message channel is currently open on both sides
    /// </summary>
    [JsonPropertyName("active")]
    public bool Active { get; set; }
    /// <summary>
    /// 2-tuple of user ids participating in direct message
    /// </summary>
    [JsonPropertyName("recipients")]
    public string[] RecipientIds { get; set; }
    /// <summary>
    /// Id of the last message sent in this channel
    /// </summary>
    [JsonPropertyName("last_message_id")]
    public string? LastMessageId { get; set; }

    public override async Task<bool> Fetch(Client client)
    {
        var data = await GetGeneric<DirectMessageChannel>(client);
        if (data == null)
            return false;

        Active = data.Active;
        RecipientIds = data.RecipientIds;
        LastMessageId = data.LastMessageId;

        return true;
    }
}
