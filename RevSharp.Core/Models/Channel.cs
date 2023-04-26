using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class BaseChannel : ISnowflake
{
    /// <summary>
    /// Unique Id
    /// </summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    /// <summary>
    /// Id of the user this channel belongs to
    /// </summary>
    [JsonPropertyName("channel_type")]
    public string ChannelType { get; set; }

    protected async Task<T?> GetGeneric<T>(string id, Client client)
    {
        var response = await client.GetAsync($"/channels/{id}");
        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Failed to fetch channel {id} (code: {response.StatusCode})");

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<T>(stringContent, Client.SerializerOptions);
        return data;
    }

    protected Task<T?> GetGeneric<T>(Client client)
        => GetGeneric<T>(Id, client);
    public BaseChannel()
    {}

    public BaseChannel(string id)
    {
        Id = id;
    }
}

/// <summary>
/// Personal "Saved Notes" channel which allows users to save messages
/// </summary>
public class SavedMessagesChannel : BaseChannel, IFetchable
{
    /// <summary>
    /// Id of the user this channel belongs to
    /// </summary>
    [JsonPropertyName("user")]
    public string UserId { get; set; }

    public async Task<bool> Fetch(Client client)
    {
        var data = await GetGeneric<SavedMessagesChannel>(client);
        if (data == null)
            return false;
        UserId = data.UserId;
        return true;
    }
}

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

    public async Task<bool> Fetch(Client client)
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

/// <summary>
/// Group channel between 1 or more participants
/// </summary>
public class GroupChannel : BaseChannel, IFetchable
{
    /// <summary>
    /// Display name of the channel
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
    /// <summary>
    /// User id of the owner of the group
    /// </summary>
    [JsonPropertyName("owner")]
    public string OwnerId { get; set; }
    /// <summary>
    /// Channel description
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    /// <summary>
    /// Array of user ids participating in channel
    /// </summary>
    [JsonPropertyName("recipients")]
    public string[] RecipientIds { get; set; }
    
    /// <summary>
    /// Custom icon attachment
    /// </summary>
    [JsonPropertyName("icon")]
    public File? Icon { get; set; }
    /// <summary>
    /// Id of the last message sent in this channel
    /// </summary>
    [JsonPropertyName("last_message_id")]
    public string? LastMessageId { get; set; }
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

    public async Task<bool> Fetch(Client client)
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

/// <summary>
/// Text channel belonging to a server
/// </summary>
public class TextChannel : BaseChannel, IFetchable
{
    /// <summary>
    /// Id of the server this channel belongs to
    /// </summary>
    [JsonPropertyName("server")]
    public string ServerId { get; set; }
    
    /// <summary>
    /// Display name of the channel
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
    /// <summary>
    /// Channel description
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    /// <summary>
    /// Custom icon attachment
    /// </summary>
    [JsonPropertyName("icon")]
    public File? Icon { get; set; }
    /// <summary>
    /// Id of the last message sent in this channel
    /// </summary>
    [JsonPropertyName("last_message_id")]
    public string LastMessageId { get; set; }
    
    /// <summary>
    /// Whether this channel is marked as not safe for work
    /// </summary>
    [JsonPropertyName("nsfw")]
    public bool IsNsfw { get; set; }

    public async Task<bool> Fetch(Client client)
    {
        var data = await GetGeneric<TextChannel>(client);
        if (data == null)
            return false;

        ServerId = data.ServerId;
        Description = data.Description;
        Icon = data.Icon;
        LastMessageId = data.LastMessageId;
        IsNsfw = data.IsNsfw;

        return true;
    }
}

public class VoiceChannel : BaseChannel, IFetchable
{
    /// <summary>
    /// Id of the server this channel belongs to
    /// </summary>
    [JsonPropertyName("server")]
    public string ServerId { get; set; }
    
    /// <summary>
    /// Display name of the channel
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
    /// <summary>
    /// Channel description
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    /// <summary>
    /// Custom icon attachment
    /// </summary>
    [JsonPropertyName("icon")]
    public File? Icon { get; set; }
    
    /// <summary>
    /// Whether this channel is marked as not safe for work
    /// </summary>
    [JsonPropertyName("nsfw")]
    public bool IsNsfw { get; set; }

    public async Task<bool> Fetch(Client client)
    {
        var data = await GetGeneric<VoiceChannel>(client);
        if (data == null)
            return false;

        ServerId = data.ServerId;
        Name = data.Name;
        Description = data.Description;
        Icon = data.Icon;
        IsNsfw = data.IsNsfw;

        return true;
    }
}