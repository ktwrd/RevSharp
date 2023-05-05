using System.Text.Json.Serialization;
using kate.shared.Helpers;

namespace RevSharp.Core.Models;

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