using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class WebPushSubscription
{
    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; }
    [JsonPropertyName("p256dh")]
    public string p256dh { get; set; }
    [JsonPropertyName("auth")]
    public string Auth { get; set; }
}