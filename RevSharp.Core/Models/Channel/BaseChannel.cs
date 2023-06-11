using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using kate.shared.Helpers;
using RevSharp.Core.Helpers;
using RevSharp.Core.Models.WebSocket;

namespace RevSharp.Core.Models;

public class BaseChannel : Clientable, IBaseChannel
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

    public virtual Task<bool> Fetch(Client client)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Fetch()
        => Fetch(Client);
    protected async Task<T?> GetGeneric<T>(string id, Client client) where T : BaseChannel
    {
        var response = await client.GetAsync($"/channels/{id}");
        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Failed to fetch channel {id} (code: {response.StatusCode})");

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<T>(stringContent, Client.SerializerOptions);
        return data;
    }

    public event GenericDelegate<string> MessageDeleted;

    internal void OnMessageDeleted(string messageId)
    {
        if (MessageDeleted != null)
            MessageDeleted?.Invoke(messageId);
    }

    /// <summary>
    /// Invoked when a user starts typing in this channel.
    /// </summary>
    public event ChannelIdDelegate StartTyping;
    /// <summary>
    /// Invoke <see cref="StartTyping"/>
    /// </summary>
    internal void OnStartTyping(string userId)
    {
        StartTyping?.Invoke(userId);
    }
    /// <summary>
    /// Invoked when a user stops typing in this channel.
    /// </summary>
    public event ChannelIdDelegate StopTyping;
    /// <summary>
    /// Invoke <see cref="StopTyping"/>
    /// </summary>
    internal void OnStopTyping(string userId)
    {
        StopTyping?.Invoke(userId);
    }

    public event VoidDelegate Deleted;
    /// <summary>
    /// Invoke <see cref="Deleted"/>
    /// </summary>
    internal void OnDeleted()
    {
        Deleted?.Invoke();
    }

    protected Task<T?> GetGeneric<T>(string id) where T : BaseChannel
        => GetGeneric<T>(id, Client);
    protected Task<T?> GetGeneric<T>(Client client) where T : BaseChannel
        => GetGeneric<T>(Id, client);

    protected Task<T?> GetGeneric<T>() where T : BaseChannel
        => GetGeneric<T>(Id, Client);

    public Task<bool> DeleteMessage(Client client, string messageId)
    {
        return Message.Delete(client, Id, messageId);
    }

    public Task<bool> DeleteMessage(string messageId)
        => DeleteMessage(Client, messageId);
    public Task<bool> DeleteMessage(Client client, Message message)
    {
        return Message.Delete(client, Id, message.Id);
    }
    public Task<bool> DeleteMessage(Message message)
        => DeleteMessage(Client, message);
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

    public Task<Message?> 
        SendMessage(
        string? content,
        Reply[]? replies,
        SendableEmbed[]? embeds,
        Masquerade? masquerade,
        Interactions[]? interactions,
        string[]? attachments)
        => SendMessage(Client, content, replies, embeds, masquerade, interactions, attachments);
    public Task<Message?> SendMessage(
        Client client,
        DataMessageSend data)
    {
        return Message.Send(client, Id, data);
    }

    public Task<Message?> SendMessage(
        Client client,
        SendableEmbed embed)
    {
        return SendMessage(
            Client, new DataMessageSend()
            {
                Content = "",
                Embeds = new []
                {
                    embed
                }
            });
    }

    public Task<Message?> SendMessage(SendableEmbed embed) => SendMessage(Client, embed);
    public Task<Message?> SendMessage(DataMessageSend data)
        => SendMessage(Client, data);

    public async Task BeginTyping(Client client)
    {
        if (client.WSClient == null)
            throw new Exception("Websocket Client not created");
        await client.WSClient.SendMessage(new TypingSendEvent(Id));
    }

    public Task BeginTyping()
        => BeginTyping(Client);

    public async Task EndTyping(Client client)
    {
        if (client.WSClient == null)
            throw new Exception("Websocket Client not created");
        await client.WSClient.SendMessage(new TypingSendEvent(Id));
    }

    public Task EndTyping()
        => EndTyping(Client);

    public async Task<ChannelInvite> CreateInvite(Client client)
    {
        var response = await client.PostAsync($"/channels/{Id}/invites", new StringContent(""));
        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Failed to create invite, server responded with {response.StatusCode}");

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<ChannelInvite>(stringContent, Client.SerializerOptions);
        if (data == null)
            throw new Exception("Failed to deserialize invite");
        return data;
    }
    
    public BaseChannel()
        : this(null, "")
    {}

    public BaseChannel(string id)
        : this(null, id)
    {
    }

    internal BaseChannel(Client? client, string id)
        : base(client)
    {
        Id = id;
        ChannelType = "Unknown";
    }

    protected override void ClientInit()
    {
        if (Client == null)
            return;
        Client.MessageReceived += OnMessageReceived;
    }

    public event MessageDelegate MessageReceived;

    private void OnMessageReceived(Message message)
    {
        MessageReceived?.Invoke(message);
    }
}