using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
using RevSharp.Core.Helpers;
using RevSharp.Core.Models;

namespace RevSharp.Core;

public partial class Client
{
    internal Dictionary<string, Server> ServerCache { get; set; }

    public async Task<LinkedList<Server>?> GetAllServers()
    {
        var list = new LinkedList<Server>();
        foreach (var item in ServerCache)
            list.AddLast(item.Value);
        return list;
    }
    
    public async Task<Server?> GetServer(string serverId, bool forceFetch = true)
    {
        var inCache = ServerCache.ContainsKey(serverId);
        if (inCache && forceFetch == false)
            return ServerCache[serverId];
        Log.Verbose($"{serverId} is " + (inCache ? "in" : "not in") + " cache");
        // Use server from cache if it exists
        var server = inCache
            ? ServerCache[serverId]
            : new Server(this, serverId);

        Log.Verbose($"{serverId} fetching");
        // Fetch latest data, add to cache if not there.
        if (!await server.Fetch()) return null;
        if (!inCache)
        {
            Log.Verbose($"{serverId} adding to cache");
            ServerCache.Add(serverId, server);
        }
        return ServerCache[serverId];
    }

    /// <returns>Was this server in the cache already</returns>
    internal bool AddToCache(Server server)
    {
        if (ServerCache.ContainsKey(server.Id))
            return true;
        ServerCache.Add(server.Id, server);
        server.Client = this;
        return false;
    }

    /// <returns>Sever Ids that were in the cache already</returns>
    internal string[] InsertIntoCache(Server[] server)
    {
        List<string> list = new List<string>();
        foreach (var i in server)
            if (AddToCache(i))
                list.Add(i.Id);
        return list.ToArray();
    }

    public Task<Server?> CreateServer(string name, string? description = null, bool nsfw = false)
    {
        if (name.Length is < 1 or > 32)
            throw new Exception("name must be less than 32 and greater than 1");
        if (description != null && description.Length is < 0 or > 1024)
            throw new Exception("description must be less than 1024 and greater than 0");
        return CreateServer(new CreateServerData()
        {
            Name = name,
            Description = description,
            IsNsfw = nsfw
        });
    }

    public async Task<Server?> CreateServer(CreateServerData data)
    {
        var response = await PostAsync(
            $"/servers/create",
            JsonContent.Create(data, options: SerializerOptions));
        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var parsed = CreateServerResponse.Parse(stringContent);

        return await GetServer(parsed.Server.Id);
    }
}

public class CreateServerData
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("nsfw")]
    public bool IsNsfw { get; set; }

    public CreateServerData()
    {
        Name = "";
        IsNsfw = false;
    }
}

public class CreateServerResponse
{
    [JsonPropertyName("server")]
    public Server Server { get; set; }
    [JsonPropertyName("Channels")]
    public BaseChannel[] Channels { get; set; }

    public static CreateServerResponse Parse(string json)
    {
        var instance = System.Text.Json.JsonSerializer.Deserialize<CreateServerResponse>(json, Client.SerializerOptions);
        var jobj = JObject.Parse(json);
        var chnls = jobj["channels"].ToArray();
        instance.Channels  = chnls
            .Select(v => ChannelHelper.ParseChannel(v.ToString()))
            .Where(v => v != null)
            .ToArray();
        return instance;
    }
}