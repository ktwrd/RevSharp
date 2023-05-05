using kate.shared.Helpers;

namespace RevSharp.Core.Models;

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