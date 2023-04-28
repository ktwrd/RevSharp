using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class VoiceChannel : BaseChannel, IFetchable
{
    /// <summary>
    /// Id of the server this channel belongs to
    /// </summary>
    [JsonPropertyName("server")]
    public string ServerId { get; set; }
    
    /// <summary>
    /// Display name of the channel
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
    /// <summary>
    /// Channel description
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    /// <summary>
    /// Custom icon attachment
    /// </summary>
    [JsonPropertyName("icon")]
    public File? Icon { get; set; }
    
    /// <summary>
    /// Whether this channel is marked as not safe for work
    /// </summary>
    [JsonPropertyName("nsfw")]
    public bool IsNsfw { get; set; }

    public async Task<bool> Fetch(Client client)
    {
        var data = await GetGeneric<VoiceChannel>(client);
        if (data == null)
            return false;

        ServerId = data.ServerId;
        Name = data.Name;
        Description = data.Description;
        Icon = data.Icon;
        IsNsfw = data.IsNsfw;

        return true;
    }
}