namespace RevSharp.Core.Models;

public partial class Message
{
    public Task<Message?> Reply(Client client, DataMessageSend data, bool mention = true)
    {
        data.Replies = new[]
        {
            new Reply(this, mention)
        };
        return Send(client, ChannelId, data);
    }
    public Task<Message?> Reply(DataMessageSend data, bool mention = true)
        => Reply(Client, data, mention);
    public Task<Message?> Reply(
        Client client,
        string? content,
        bool mention = true,
        SendableEmbed[]? embeds = null,
        Masquerade? masquerade = null,
        Interactions[]? interactions = null,
        string[]? attachments = null)
    {
        var data = new DataMessageSend()
        {
            Replies = new []
            {
                new Reply(this, mention)
            },
            Content = content,
            Embeds = embeds,
            Masquerade = masquerade,
            Interactions = interactions,
            Attachments = attachments
        };
        return Reply(client, data);
    }

    public Task<Message?> Reply(
        SendableEmbed embed,
        bool mention = true,
        Masquerade? masquerade = null,
        Interactions[]? interactions = null,
        string[]? attachments = null)
        => Reply(
            Client,
            "",
            mention,
            new[] { embed },
            masquerade,
            interactions,
            attachments);
    public Task<Message?> Reply(
        Client client,
        SendableEmbed embed,
        bool mention = true,
        Masquerade? masquerade = null,
        Interactions[]? interactions = null,
        string[]? attachments = null)
        => Reply(
            client,
            "",
            mention,
            new[] { embed },
            masquerade,
            interactions,
            attachments);
    public Task<Message?> Reply(
        string? content,
        bool mention = true,
        SendableEmbed[]? embeds = null,
        Masquerade? masquerade = null,
        Interactions[]? interactions = null,
        string[]? attachments = null)
        => Reply(
            Client,
            content,
            mention,
            embeds,
            masquerade,
            interactions,
            attachments);
}