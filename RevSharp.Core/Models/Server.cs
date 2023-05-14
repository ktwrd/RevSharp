using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public partial class Server : Clientable, ISnowflake, IFetchable
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    [JsonPropertyName("owner")]
    public string OwnerId { get; set; }
    [JsonIgnore]
    public User? Owner { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("channels")]
    public string[] ChannelIds { get; set; }
    [JsonPropertyName("categories")]
    public ServerCategory[] Categories { get; set; }
    [JsonPropertyName("system_messages")]
    public Dictionary<string, string> SystemMessageChannels { get; set; }
    
    [JsonPropertyName("roles")]
    public Dictionary<string, ServerRole> Roles { get; set; }
    [JsonPropertyName("default_permissions")]
    public long DefaultPermissions { get; set; }
    
    [JsonPropertyName("icon")]
    public File? Icon { get; set; }
    [JsonPropertyName("banner")]
    public File? Banner { get; set; }
    
    [JsonPropertyName("flags")]
    public long? Flags { get; set; }
    
    [JsonPropertyName("nsfw")]
    public bool IsNsfw { get; set; }
    [JsonPropertyName("analytics")]
    public bool EnableAnalytics { get; set; }
    [JsonPropertyName("discoverable")]
    public bool IsDiscoverable { get; set; }
    public List<Member> Members { get; set; }
    internal static async Task<Server?> Get(string id, Client client, bool fetchOwner = true)
    {
        var response = await client.GetAsync($"/servers/{id}");
        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Failed to fetch server {id} (code: {response.StatusCode})");

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<Server>(stringContent, Client.SerializerOptions);
        if (fetchOwner && data != null)
        {
            var owner = new User(data.OwnerId);
            var ownerSuccess = await owner.Fetch(client);
            if (ownerSuccess)
                data.Owner = owner;
        }
        return data;
    }
    public async Task<bool> Fetch(Client client)
    {
        var data = await Get(Id, client);
        if (data == null)
            return false;

        var members = await FetchMembers(client);
        if (members == null)
            return false;
        data.Members = members.ToList();
        Inject(data, this);

        return true;
    }

    public async Task<Member[]?> FetchMembers(Client client)
    {
        var response = await client.GetAsync($"/servers/{Id}/members");
        if (response.StatusCode != HttpStatusCode.OK)
            return null;
        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<ServerMemberResult>(stringContent, Client.SerializerOptions);
        foreach (var i in data.Members)
            i.Client = Client;
        return data.Members;
    }

    internal static void Inject(Server source, Server target)
    {
        target.OwnerId = source.OwnerId;
        target.Owner = source.Owner;
        target.Name = source.Name;
        target.Description = source.Description;
        target.ChannelIds = source.ChannelIds;
        target.Categories = source.Categories;
        target.SystemMessageChannels = source.SystemMessageChannels;
        target.Roles = source.Roles;
        target.DefaultPermissions = source.DefaultPermissions;
        target.Icon = source.Icon;
        target.Banner = source.Banner;
        target.Flags = source.Flags;
        target.IsNsfw = source.IsNsfw;
        target.EnableAnalytics = source.EnableAnalytics;
        target.IsDiscoverable = source.IsDiscoverable;
    }

    public Task<bool> Fetch()
        => Fetch(Client);
    
    public Server()
        : this(null, "")
    {}

    public Server(string id)
        : this(null, id)
    {
    }

    internal Server(Client client, string id)
        : base(client)
    {
        Id = id;
        Members = new List<Member>();
    }
}

