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

    public Task<Message?> SendMessage(
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
    public Task<Message?> SendMessage(DataMessageSend data)
        => SendMessage(Client, data);
    
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