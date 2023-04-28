using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using RevSharp.Core.Helpers;

namespace RevSharp.Core.Models;

public class BaseChannel : Clientable, ISnowflake
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

    protected async Task<T?> GetGeneric<T>(string id, Client client) where T : BaseChannel
    {
        var response = await client.GetAsync($"/channels/{id}");
        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Failed to fetch channel {id} (code: {response.StatusCode})");

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<T>(stringContent, Client.SerializerOptions);
        return data;
    }

    protected Task<T?> GetGeneric<T>(string id) where T : BaseChannel
        => GetGeneric<T>(id, _client);
    protected Task<T?> GetGeneric<T>(Client client) where T : BaseChannel
        => GetGeneric<T>(Id, client);

    protected Task<T?> GetGeneric<T>() where T : BaseChannel
        => GetGeneric<T>(Id, _client);

    public Task<bool> DeleteMessage(Client client, string messageId)
    {
        return Message.Delete(client, Id, messageId);
    }

    public Task<bool> DeleteMessage(string messageId)
        => DeleteMessage(_client, messageId);
    public Task<bool> DeleteMessage(Client client, Message message)
    {
        return Message.Delete(client, Id, message.Id);
    }
    public Task<bool> DeleteMessage(Message message)
        => DeleteMessage(_client, message);
    public async Task<Message?> GetMessage(Client client, string id)
    {
        var message = new Message
        {
            Id = id,
            ChannelId = Id
        };
        if (await message.Fetch(client))
            return message;
        return null;
    }
    
    public Task<Message?> SendMessage(
        Client client,
        string? content,
        Reply[]? replies,
        SendableEmbed[]? embeds,
        Masquerade? masquerade,
        Interactions[]? interactions,
        string[]? attachments)
    {
        var data = new DataMessageSend()
        {
            Content = content,
            Attachments = attachments,
            Replies = replies,
            Embeds = embeds,
            Masquerade = masquerade,
            Interactions = interactions
        };
        return SendMessage(client, data);
    }

    public Task<Message?> SendMessage(
        string? content,
        Reply[]? replies,
        SendableEmbed[]? embeds,
        Masquerade? masquerade,
        Interactions[]? interactions,
        string[]? attachments)
        => SendMessage(_client, content, replies, embeds, masquerade, interactions, attachments);
    public Task<Message?> SendMessage(
        Client client,
        DataMessageSend data)
    {
        return Message.Send(client, Id, data);
    }
    public Task<Message?> SendMessage(DataMessageSend data)
        => SendMessage(_client, data);
    
    public BaseChannel()
        : this(null, "")
    {}

    public BaseChannel(string id)
        : this(null, id)
    {
    }

    public BaseChannel(Client client, string id)
        : base(client)
    {
        Id = id;
        if (client != null)
        {
            client.MessageReceived += (m) =>
            {
                if (m.ChannelId == Id)
                    MessageReceived?.Invoke(m);
            };
        }
    }

    public event MessageDelegate MessageReceived;
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