using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
using RevSharp.Core.Helpers;

namespace RevSharp.Core.Models.WebSocket;

public class MessageAppendEvent : BaseTypedResponse
{
    [JsonPropertyName("id")]
    public string MessageId { get; set; }
    [JsonPropertyName("channel")]
    public string ChannelId { get; set; }
    [JsonPropertyName("append")]
    public MessageAppendEventData Append { get; set; }

    public static MessageAppendEvent? Parse(string content)
    {
        var data = JsonSerializer.Deserialize<MessageAppendEvent>(content, Client.SerializerOptions);
        if (data == null)
            return null;
        var jobj = JObject.Parse(content);
        var apn = jobj["append"];
        var ebm = apn["embeds"] ?? null;
        if (apn != null && ebm != null)
            data.Append.Embeds = MessageHelper.ParseEmbeds(ebm.ToArray());
        return data;
    }
}

public class MessageAppendEventData
{
    [JsonPropertyName("embeds")]
    public BaseEmbed[]? Embeds { get; set; }
}