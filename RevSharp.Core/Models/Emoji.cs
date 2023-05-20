using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

/// <summary>
/// Representation of an Emoji on Revolt
/// </summary>
public class Emoji : IFetchable, ISnowflake
{
    /// <summary>
    /// Unique Id
    /// </summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    /// <summary>
    /// What owns this emoji
    /// </summary>
    [JsonPropertyName("parent")]
    public EmojiParent Parent { get; set; }
    /// <summary>
    /// Uploader user id
    /// </summary>
    [JsonPropertyName("creator_id")]
    public string CreatorId { get; set; }
    [JsonIgnore]
    public User Creator { get; set; }
    /// <summary>
    /// Emoji name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
    /// <summary>
    /// Whether the emoji is animated
    /// </summary>
    [JsonPropertyName("animated")]
    public bool IsAnimated { get; set; }
    /// <summary>
    /// Whether the emoji is marked as nsfw
    /// </summary>
    [JsonPropertyName("nsfw")]
    public bool IsNsfw { get; set; }

    public static async Task<Emoji?> Get(string id, Client client, bool fetchCreator = true)
    {
        // Fetch emoji data
        var response = await client.GetAsync($"/custom/emoji/{id}");
        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Failed to fetch emoji id {id} (code: {response.StatusCode})");

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<Emoji>(stringContent, Client.SerializerOptions);
        if (fetchCreator && data != null)
        {
            await data.FetchCreator(client);
        }
        return data;
    }
    
    public async Task<bool> Fetch(Client client)
    {
        var data = await Get(Id, client);
        if (data == null)
            return false;

        Parent = data.Parent;
        CreatorId = data.CreatorId;
        Name = data.Name;
        IsAnimated = data.IsAnimated;
        IsNsfw = data.IsNsfw;
        
        // Fetch creator
        var creatorSuccess = await FetchCreator(client);
        if (!creatorSuccess)
            throw new Exception("Failed to fetch emoji creator");

        return true;
    }

    internal async Task<bool> FetchCreator(Client client)
    {
        var creator = new User(CreatorId);
        var creatorSuccess = await creator.Fetch(client);
        if (creatorSuccess)
            Creator = creator;
        return creatorSuccess;
    }
}
