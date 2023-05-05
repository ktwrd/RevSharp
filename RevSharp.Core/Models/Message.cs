using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using kate.shared.Helpers;
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
        Client = client;
        Id = id;
        ChannelId = channelId;
        
        Reactions = new Dictionary<string, string[]>();
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

    public Task<bool> Fetch()
        => Fetch(Client);
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

    public event MessageReactedDelegate ReactAdd;
    internal void OnReactAdd(string userId, string react)
    {
        if (Reactions.ContainsKey(react))
        {
            Reactions[react] = Reactions[react].Concat(new string[] { userId }).ToArray();
        }
        else
        {
            Reactions.Add(react, new string[]
            {
                userId
            });
        }
        ReactAdd?.Invoke(userId, react, Id);
    }

    public event MessageReactedDelegate ReactRemove;

    internal void OnReactRemove(string userId, string react)
    {
        if (Reactions.ContainsKey(react))
            Reactions[react] = Reactions[react].Where(v => v != userId).ToArray();
        ReactRemove?.Invoke(userId, react, Id);
    }
}

public class MessageBuilder
{
    private string? _content;
    private List<string> _attachments;
    private List<Reply> _replies;
    private List<EmbedBuilder> _embeds;
    private Masquerade? _masquerade;
    public MessageBuilder WithContent(string content)
    {
        if (content.Length > 2000)
            throw new Exception("Must be less than 200 characters");
        _content = content;
        return this;
    }

    public MessageBuilder ReplyTo(string messageId, bool mention = true)
    {
        _replies.Add(new Reply(messageId, mention));
        return this;
    }

    public MessageBuilder MasqueradeAs(string name, string? avatarUrl = null, string? colour = null)
    {
        _masquerade = new Masquerade()
        {
            Name = name,
            Avatar = avatarUrl,
            Colour = colour
        };
        return this;
    }

    public MessageBuilder AddEmbed(EmbedBuilder builder)
    {
        if (_embeds.Count >= 10)
        {
            throw new Exception("Maximum length of Embeds must be 10");
        }
        _embeds.Add(builder);
        return this;
    }

    public SendableMessage Build()
    {
        return new SendableMessage()
        {
            Nonce = GeneralHelper.GenerateUID(),
            Content = _content,
            Attachments = _attachments.ToArray(),
            Replies = _replies.ToArray(),
            Embeds = _embeds.Select(v => v as SendableEmbed).ToArray(),
            Masquerade = _masquerade
        };
    }

    public MessageBuilder()
    {
        _content = null;
        _attachments = new List<string>();
        _replies = new List<Reply>();
        _embeds = new List<EmbedBuilder>();
        _masquerade = null;
    }
}

public class SendableMessage
{
    [JsonPropertyName("nonce")]
    public string? Nonce { get; set; }
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    [JsonPropertyName("replies")]
    public Reply[]? Replies { get; set; }
    [JsonPropertyName("embeds")]
    public SendableEmbed[]? Embeds { get; set; }
    [JsonPropertyName("masquerade")]
    public Masquerade? Masquerade { get; set; }
    [JsonPropertyName("attachments")]
    public string[] Attachments { get; set; }

    public SendableMessage()
    {
        Nonce = GeneralHelper.GenerateUID();
        Replies = Array.Empty<Reply>();
        Embeds = Array.Empty<SendableEmbed>();
    }
}
