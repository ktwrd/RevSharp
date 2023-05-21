using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;


/// <summary>
/// Group channel between 1 or more participants
/// </summary>
public class GroupChannel : MessageableChannel, IFetchable
{
    /// <summary>
    /// User id of the owner of the group
    /// </summary>
    [JsonPropertyName("owner")]
    public string OwnerId { get; set; }
    /// <summary>
    /// Array of user ids participating in channel
    /// </summary>
    [JsonPropertyName("recipients")]
    public string[] RecipientIds { get; set; }
    
    /// <summary>
    /// Permissions assigned to members of this group
    /// (does not apply to the owner of the group)
    /// </summary>
    [JsonPropertyName("permissions")]
    public long? Permissions { get; set; }
    
    /// <summary>
    /// Whether this group is marked as not safe for work
    /// </summary>
    [JsonPropertyName("nsfw")]
    public bool IsNsfw { get; set; }

    public override async Task<bool> Fetch(Client client)
    {
        var data = await GetGeneric<GroupChannel>(client);
        if (data == null)
            return false;

        Name = data.Name;
        OwnerId = data.OwnerId;
        Description = data.Description;
        RecipientIds = data.RecipientIds;
        Icon = data.Icon;
        LastMessageId = data.LastMessageId;
        Permissions = data.Permissions;
        IsNsfw = data.IsNsfw;

        return true;
    }
}