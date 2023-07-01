using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

/// <summary>
/// Data that gets sent to Revolt's API when sending a message.
/// </summary>
public class DataMessageSend
{
    /// <summary>
    /// Set automatically in constructor. Don't worry about setting this.
    /// Either way, it's been deprecated and doesn't really matter.
    /// </summary>
    [JsonPropertyName("nonce")]
    public string? Nonce { get; set; }
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    /// <summary>
    /// Array of Attachment Ids from Autumn. Use <see cref="RevSharp.Core.Client.UploadFile"/>
    /// </summary>
    [JsonPropertyName("attachments")]
    public string[]? Attachments { get; set; }
    /// <summary>
    /// Messages to reply to. If you'd like to easily reply to messages, use <see cref="AddReply(string,bool)"/>
    /// </summary>
    [JsonPropertyName("replies")]
    public Reply[]? Replies { get; set; }
    /// <summary>
    /// Embeds to include in this message. Please use <see cref="AddEmbed"/>
    /// </summary>
    [JsonPropertyName("embeds")]
    public SendableEmbed[]? Embeds { get; set; }
    /// <summary>
    /// Masquerade this message.
    /// </summary>
    [JsonPropertyName("masquerade")]
    public Masquerade? Masquerade { get; set; }
    /// <summary>
    /// TODO: Not sure what this is for.
    /// </summary>
    [JsonPropertyName("interactions")]
    public Interactions[]? Interactions { get; set; }

    /// <summary>
    /// Add an embed to this message
    /// </summary>
    /// <param name="embed">Embed to add</param>
    /// <returns>Instance of this</returns>
    public DataMessageSend AddEmbed(SendableEmbed embed)
    {
        Embeds = Embeds?.Concat(new SendableEmbed[] { embed }).ToArray();
        return this;
    }
    /// <summary>
    /// Add an attachment id to this message
    /// </summary>
    /// <param name="id">Attachment Id to add</param>
    /// <returns>Instance of this</returns>
    public DataMessageSend AddAttachment(string id)
    {
        if (Attachments == null)
            Attachments = Array.Empty<string>();
        Attachments = Attachments?.Concat(new string[] { id }).ToArray();
        return this;
    }
    /// <summary>
    /// Add a reply to this message
    /// </summary>
    /// <param name="id">Message Id</param>
    /// <param name="mention">Mention message</param>
    /// <returns>Instance of this</returns>
    public DataMessageSend AddReply(string id, bool mention = true)
    {
        Replies = Replies?.Concat(new Reply[]
        {
            new Reply()
            {
                Id = id,
                Mention = mention
            }
        }).ToArray();
        return this;
    }
    /// <summary>
    /// Add a reply to this message
    /// </summary>
    /// <param name="message">Message that we want to reply to</param>
    /// <param name="mention">Mention message</param>
    /// <returns>Instance of this</returns>
    public DataMessageSend AddReply(Message message, bool mention = true)
        => AddReply(message.Id, mention);

    /// <summary>
    /// Sets <see cref="Content"/>. Will override this field if already set.
    /// </summary>
    /// <param name="text">Content to set</param>
    /// <returns>Instance of this</returns>
    public DataMessageSend WithContent(string text)
    {
        Content = text;
        return this;
    }
}