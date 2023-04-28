using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class Reply : ISnowflake
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("mention")]
    public bool Mention { get; set; }

    public Reply()
    {
        Id = "";
        Mention = true;
    }

    public Reply(Message message, bool mention = true)
    {
        Id = message.Id;
        Mention = mention;
    }
}