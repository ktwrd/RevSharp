using System.Text.Json.Serialization;
using kate.shared.Helpers;

namespace RevSharp.Core.Models;

public class SendableMessage
{
    /// <summary>
    /// Unique token to prevent duplicate message sending
    /// </summary>
    [JsonPropertyName("nonce")]
    public string? Nonce { get; set; }
    /// <summary>
    /// Message content to send. Maximum length is 2000
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    /// <summary>
    /// Attachments to include in message
    /// </summary>
    [JsonPropertyName("attachments")]
    public string[] Attachments { get; set; }
    /// <summary>
    /// Messages to reply to
    /// </summary>
    [JsonPropertyName("replies")]
    public Reply[]? Replies { get; set; }
    /// <summary>
    /// Embeds to include in message
    ///
    /// Text embed content contributes to the content length cap
    /// </summary>
    [JsonPropertyName("embeds")]
    public SendableEmbed[]? Embeds { get; set; }
    /// <summary>
    /// Masquerade to apply to this message
    /// </summary>
    [JsonPropertyName("masquerade")]
    public Masquerade? Masquerade { get; set; }

    /// <summary>
    /// Information about how this message should be interacted with
    /// </summary>
    [JsonPropertyName("interactions")]
    public Interactions? Interactions { get; set; }
    
    public SendableMessage()
    {
        Nonce = GeneralHelper.GenerateUID();
        Replies = Array.Empty<Reply>();
        Embeds = Array.Empty<SendableEmbed>();
    }
}