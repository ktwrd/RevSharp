using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using RevSharp.Core.Helpers;

namespace RevSharp.Core.Models;

public partial class Message : Clientable, ISnowflake
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    [JsonPropertyName("nonce")]
    public string? Nonce { get; set; }
    [JsonPropertyName("channel")]
    public string ChannelId { get; set; }
    [JsonPropertyName("author")]
    public string AuthorId { get; set; }
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    [JsonPropertyName("system")]
    public SystemMessage? SystemMessage { get; set; }
    [JsonPropertyName("attachments")]
    public File[]? Attachments { get; set; }
    [JsonPropertyName("edited")]
    public string? EditedAt { get; set; }
    [JsonPropertyName("embeds")]
    public BaseEmbed[]? Embeds { get; set; }
    [JsonPropertyName("mentions")]
    public string[]? MentionIds { get; set; }
    [JsonPropertyName("replies")]
    public string[]? MessageReplyIds { get; set; }
    [JsonPropertyName("reactions")]
    public Dictionary<string, string[]> Reactions { get; set; }
    [JsonPropertyName("interactions")]
    public Interactions Interactions { get; set; }
    [JsonPropertyName("Masquerade")]
    public Masquerade? Masquerade { get; set; }

    public Message()
        : this(null, "", "")
    {}
    public Message(string id, string channelId)
        : this(null, id, channelId)
    {}

    internal Message(Client? client, string id, string channelId)
        : base(client)
    {
        Id = id;
        ChannelId = channelId;
    }

    internal async Task<Message?> Fetch(Client client, string channelId, string messageId)
    {
        var message = new Message
        {
            ChannelId = channelId,
            Id = messageId
        };
        if (await message.Fetch(client))
            return message;
        return null;
    }
    public async Task<bool> Fetch(Client client)
    {
        var response = await client.GetAsync($"/channels/{ChannelId}/messages/{Id}");
        if (response.StatusCode != HttpStatusCode.OK)
            return false;

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = Parse(stringContent);
        if (data == null)
            return false;
        
        Inject(data, this);
        return true;
    }
    public static Message? Parse(string content)
    {
        var data = JsonSerializer.Deserialize<Message>(content, Client.SerializerOptions);
        if (data == null)
            return null;
        data.Embeds = MessageHelper.ParseMessageEmbeds(content);
        data.SystemMessage = MessageHelper.ParseSystemMessage(content);
        return data;
    }

    public static void Inject(Message source, Message target)
    {
        target.Id = source.Id;
        target.Nonce = source.Nonce;
        target.ChannelId = source.ChannelId;
        target.AuthorId = source.AuthorId;
        target.Content = source.Content;
        target.SystemMessage = source.SystemMessage;
        target.Attachments = source.Attachments;
        target.EditedAt = source.EditedAt;
        target.Embeds = source.Embeds;
        target.MentionIds = source.MentionIds;
        target.MessageReplyIds = source.MessageReplyIds;
        target.Reactions = source.Reactions;
        target.Interactions = source.Interactions;
        target.Masquerade = source.Masquerade;
    }
}

public class DataMessageSend
{
    [JsonPropertyName("nonce")]
    public string? Nonce { get; set; }
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    [JsonPropertyName("attachments")]
    public string[]? Attachments { get; set; }
    [JsonPropertyName("replies")]
    public Reply[]? Replies { get; set; }
    [JsonPropertyName("embeds")]
    public SendableEmbed[]? Embeds { get; set; }
    [JsonPropertyName("masquerade")]
    public Masquerade? Masquerade { get; set; }
    [JsonPropertyName("interactions")]
    public Interactions[]? Interactions { get; set; }
}