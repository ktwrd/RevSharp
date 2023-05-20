using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

/// <summary>
/// Representation of a server on Revolt
/// </summary>
public partial class Server : Clientable, ISnowflake, IFetchable
{
    /// <summary>
    /// Unique Id
    /// </summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    /// <summary>
    /// User id of the owner
    /// </summary>
    [JsonPropertyName("owner")]
    public string OwnerId { get; set; }
    [JsonIgnore]
    public User? Owner { get; set; }
    
    /// <summary>
    /// Name of the server
    /// </summary>
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    /// <summary>
    /// Description for the server
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    /// <summary>
    /// Channels within this server
    /// </summary>
    [JsonPropertyName("channels")]
    public string[] ChannelIds { get; set; }
    /// <summary>
    /// Categories for this server
    /// </summary>
    [JsonPropertyName("categories")]
    public ServerCategory[] Categories { get; set; }
    /// <summary>
    /// Configuration for sending system event messages.
    ///
    /// Key: Event Name
    /// Value: Channel Id
    /// </summary>
    [JsonPropertyName("system_messages")]
    public Dictionary<string, string> SystemMessageChannels { get; set; }
    
    /// <summary>
    /// Roles for this server
    /// </summary>
    [JsonPropertyName("roles")]
    public Dictionary<string, ServerRole> Roles { get; set; }
    /// <summary>
    /// Default set of server and channel permissions
    /// </summary>
    [JsonPropertyName("default_permissions")]
    public long DefaultPermissions { get; set; }
    
    /// <summary>
    /// Icon attachment
    /// </summary>
    [JsonPropertyName("icon")]
    public File? Icon { get; set; }
    /// <summary>
    /// Banner attachment
    /// </summary>
    [JsonPropertyName("banner")]
    public File? Banner { get; set; }
    
    /// <summary>
    /// Bitfield of server flags
    /// </summary>
    [JsonPropertyName("flags")]
    public long? Flags { get; set; }
    
    /// <summary>
    /// Whether this server is flagged as not safe for work
    /// </summary>
    [JsonPropertyName("nsfw")]
    public bool IsNsfw { get; set; }
    /// <summary>
    /// Whether to enable analytics
    /// </summary>
    [JsonPropertyName("analytics")]
    public bool EnableAnalytics { get; set; }
    /// <summary>
    /// Whether this server should be publicly discoverable
    /// </summary>
    [JsonPropertyName("discoverable")]
    public bool IsDiscoverable { get; set; }
    
    /// <summary>
    /// List of parsed members
    /// </summary>
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
    
    /// <summary>
    /// Fetch latest data about this server from the API and insert into this instance
    /// </summary>
    /// <returns>Did it successfully fetch and inject from the API</returns>
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

    /// <summary>
    /// Fetch members from the API
    /// </summary>
    /// <returns>Array of members</returns>
    public async Task<Member[]?> FetchMembers(Client client)
    {
        var response = await client.GetAsync($"/servers/{Id}/members");
        if (response.StatusCode != HttpStatusCode.OK)
            return null;
        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<ServerMemberResult>(stringContent, Client.SerializerOptions);
        foreach (var i in data.Members)
            i.Client = Client;
        Members = data.Members.ToList();
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

    /// <summary>
    /// Fetch latest data about this server from the API and insert into this instance
    /// </summary>
    /// <returns>Did it successfully fetch and inject from the API</returns>
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

