using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class SendableEmbed
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("icon_url")]
    public string? IconUrl { get; set; }
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    [JsonPropertyName("media")]
    public string? Media { get; set; }
    [JsonPropertyName("colour")]
    public string? Colour { get; set; }
}

public class EmbedBuilder : SendableEmbed
{
    public SendableEmbed WithDescription(string description)
    {
        Description = description;
        return this;
    }

    public SendableEmbed WithIconUrl(string iconUrl)
    {
        IconUrl = iconUrl;
        return this;
    }

    public SendableEmbed WithUrl(string url)
    {
        Url = url;
        return this;
    }

    public SendableEmbed WithTitle(string title)
    {
        Title = title;
        return this;
    }

    public SendableEmbed WithMedia(string url)
    {
        Media = url;
        return this;
    }

    public SendableEmbed WithColour(string colour)
    {
        Colour = colour;
        return this;
    }
}

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