using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class RevoltNodeFeatures
{
    [JsonPropertyName("captcha")]
    public CaptchaFeature Captcha { get; set; }
    [JsonPropertyName("email")]
    public bool Email { get; set; }
    [JsonPropertyName("invite_only")]
    public bool InviteOnly { get; set; }
    [JsonPropertyName("autumn")]
    public UrlFeature Autumn { get; set; }
    [JsonPropertyName("january")]
    public UrlFeature January { get; set; }
    [JsonPropertyName("voso")]
    public UrlFeature Voso { get; set; }
}

public class CaptchaFeature
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
    [JsonPropertyName("key")]
    public string Key { get; set; }
}

public class UrlFeature
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; }
    [JsonPropertyName("ws")]
    public string? WebSocket { get; set; }
}