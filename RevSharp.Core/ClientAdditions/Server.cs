using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
using RevSharp.Core.Helpers;
using RevSharp.Core.Models;

namespace RevSharp.Core;

public partial class Client
{
    internal Dictionary<string, Member> MemberCache { get; set; }

    public async Task<LinkedList<Member>?> GetAllMembers()
    {
        var list = new LinkedList<Member>();
        foreach (var item in MemberCache)
            list.AddLast(item.Value);
        return list;
    }

    /// <summary>
    /// Add server member to cache
    /// </summary>
    /// <param name="member">Target member to attempt to add to the cache</param>
    /// <returns>Is this member in the cache already?</returns>
    internal bool AddToCache(Member member)
    {
        var key = member.Id.UserId + member.Id.ServerId;
        if (MemberCache.ContainsKey(key))
            return true;
        MemberCache.TryAdd(key, member);
        member.Client = this;
        return false;
    }

    /// <summary>
    /// Get server member
    /// </summary>
    /// <param name="serverId">Server Id the member belongs to</param>
    /// <param name="userId">User Id to fetch</param>
    /// <param name="forceUpdate">Force an update from the API</param>
    /// <returns>Returns `null` when failed to fetch or something else bad happens.</returns>
    public async Task<Member?> GetMember(string serverId, string userId, bool forceUpdate = true)
    {
        var key = userId + serverId;
        var inCache = MemberCache.ContainsKey(key);
        if (inCache && !forceUpdate)
            return MemberCache[key];

        var server = await GetServer(serverId, forceUpdate);
        var member = inCache ? MemberCache[key] : new Member(this, serverId, userId);
        if (!await member.Fetch())
            return null;
        
        if (!inCache)
            MemberCache.Add(key, member);
        return MemberCache[key];
    }
    internal Dictionary<string, Server> ServerCache { get; set; }

    /// <summary>
    /// Transform the ServerCache into a LinkedList
    /// </summary>
    /// <returns>LinkedList of Servers.</returns>
    public async Task<LinkedList<Server>?> GetAllServers()
    {
        var list = new LinkedList<Server>();
        foreach (var item in ServerCache)
        {
            if (item.Value.Client == null)
            {
                ServerCache[item.Key].Client = this;
                item.Value.Client = this;
            }
            list.AddLast(item.Value);
        }
        return list;
    }
    
    /// <summary>
    /// Get a server from the current Revolt server.
    /// </summary>
    /// <param name="serverId">Server Id to fetch the data for</param>
    /// <param name="forceUpdate">When `true`, the cache will be ignored and it will fetch directly from the API, like if it was never in the cache to start with</param>
    /// <returns>Server specified or null</returns>
    public async Task<Server?> GetServer(string serverId, bool forceUpdate = true)
    {
        var inCache = ServerCache.ContainsKey(serverId);
        if (inCache && forceUpdate == false)
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
            ServerCache.TryAdd(serverId, server);
        }
        return ServerCache[serverId];
    }

    /// <returns>Was this server in the cache already</returns>
    internal bool AddToCache(Server server)
    {
        if (ServerCache.ContainsKey(server.Id))
            return true;
        ServerCache.TryAdd(server.Id, server);
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

    /// <summary>
    /// Create a server. This can only be accessed if you are not a bot.
    /// </summary>
    /// <param name="name">Name of the server</param>
    /// <param name="description">Server description</param>
    /// <param name="nsfw">Is this an NSFW server</param>
    /// <returns>`null` when failed to create server.</returns>
    /// <exception cref="Exception">When parameter validation fails or you're logged in as a bot</exception>
    public Task<Server?> CreateServer(string name, string? description = null, bool nsfw = false)
    {
        if (CurrentUser?.Bot != null)
            throw new Exception("Only users can create servers");
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

    /// <summary>
    /// Create a server with <see cref="CreateServerData"/>
    /// </summary>
    /// <param name="data">Data to create the server with</param>
    /// <returns>`null` when failed to create the server.</returns>
    /// <exception cref="Exception">Thrown when you're logged in as a bot.</exception>
    public async Task<Server?> CreateServer(CreateServerData data)
    {
        if (CurrentUser?.Bot != null)
            throw new Exception("Only users can create servers");
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