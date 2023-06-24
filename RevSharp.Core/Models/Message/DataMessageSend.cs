using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

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
}