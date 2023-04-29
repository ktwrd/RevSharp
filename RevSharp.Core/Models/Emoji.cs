using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class Emoji : IFetchable, ISnowflake
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    [JsonPropertyName("parent")]
    public EmojiParent Parent { get; set; }
    [JsonPropertyName("creator_id")]
    public string CreatorId { get; set; }
    [JsonIgnore]
    public User Creator { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("animated")]
    public bool IsAnimated { get; set; }
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
