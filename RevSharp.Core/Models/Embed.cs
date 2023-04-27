using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class BaseEmbed
{
    public string Type { get; set; }
}

public class TextEmbed : BaseEmbed
{
    [JsonPropertyName("icon_url")]
    public string? IconUrl { get; set; }
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("media")]
    public File? Media { get; set; }
    [JsonPropertyName("colour")]
    public string? Colour { get; set; }
}

public class MetadataEmbed : BaseEmbed
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    [JsonPropertyName("original_url")]
    public string? OriginalUrl { get; set; }
    [JsonPropertyName("special")]
    public SpecialEmbed? Special { get; set; }
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("image")]
    public ImageEmbed? Image { get; set; }
    [JsonPropertyName("video")]
    public VideoEmbed? Video { get; set; }
    [JsonPropertyName("site_name")]
    public string? SiteName { get; set; }
    [JsonPropertyName("icon_url")]
    public string? IconUrl { get; set; }
    [JsonPropertyName("colour")]
    public string? Colour { get; set; }
}

public class VideoEmbed : BaseEmbed
{
    [JsonPropertyName("url")]
    public string Url { get; set; }
    [JsonPropertyName("width")]
    public int Width { get; set; }
    [JsonPropertyName("height")]
    public int Height { get; set; }
}

public class ImageEmbed : BaseEmbed
{
    [JsonPropertyName("url")]
    public string Url { get; set; }
    [JsonPropertyName("width")]
    public int Width { get; set; }
    [JsonPropertyName("height")]
    public int Height { get; set; }
    [JsonPropertyName("size")]
    public ImageSize Size { get; set; }
}

public enum ImageSize
{
    Large,
    Preview
}
public class SpecialEmbed
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }
    [JsonPropertyName("content_type")]
    public string? ContentType { get; set; }
}

public static class EmbedHelper
{
    public static BaseEmbed[] GetEmbeds(string json)
    {
        var list = new List<BaseEmbed>();

        var metadataEmbedList = JsonSerializer.Deserialize<MetadataEmbed[]>(json, Client.SerializerOptions)
            .Where(v => v.Type == "Website");
        list = list.Concat(metadataEmbedList).ToList();

        var imageEmbedList = JsonSerializer.Deserialize<ImageEmbed[]>(json, Client.SerializerOptions)
            .Where(v => v.Type == "Image");
        list = list.Concat(imageEmbedList).ToList();

        var videoEmbedList = JsonSerializer.Deserialize<VideoEmbed[]>(json, Client.SerializerOptions)
            .Where(v => v.Type == "Video");
        list = list.Concat(videoEmbedList).ToList();
        
        var textEmbedList = JsonSerializer.Deserialize<TextEmbed[]>(json, Client.SerializerOptions)
            .Where(v => v.Type == "Text");
        list = list.Concat(textEmbedList).ToList();

        return list.ToArray();
    }

    public static BaseEmbed[] GetEmbedsFromMessage(string json)
    {
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json, Client.SerializerOptions);
        if (dict?.ContainsKey("embeds") ?? false)
        {
            return GetEmbeds(JsonSerializer.Serialize(dict["embeds"], Client.SerializerOptions));
        }

        return Array.Empty<BaseEmbed>();
    }
}