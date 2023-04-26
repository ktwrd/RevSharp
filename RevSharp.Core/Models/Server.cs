﻿using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class Server : ISnowflake, IFetchable
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

    public static async Task<Server?> Get(string id, Client client, bool fetchOwner = true)
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

        OwnerId = data.OwnerId;
        Owner = data.Owner;
        Name = data.Name;
        Description = data.Description;
        ChannelIds = data.ChannelIds;
        Categories = data.Categories;
        SystemMessageChannels = data.SystemMessageChannels;
        Roles = data.Roles;
        DefaultPermissions = data.DefaultPermissions;
        Icon = data.Icon;
        Banner = data.Banner;
        Flags = data.Flags;
        IsNsfw = data.IsNsfw;
        EnableAnalytics = data.EnableAnalytics;
        IsDiscoverable = data.IsDiscoverable;

        return true;
    }
}

public enum ServerFlags
{
    Verified = 1,
    Official = 2
}
public class ServerCategory
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("channels")]
    public string[] ChannelIds { get; set; }
}

public class ServerRole
{
    /// <summary>
    /// Role Name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
    /// <summary>
    /// This can be any valid CSS colour
    /// </summary>
    [JsonPropertyName("colour")]
    public string? Colour { get; set; }
    /// <summary>
    /// Whether this role should be shown separately on the member sidebar
    /// </summary>
    [JsonPropertyName("hoist")]
    public bool Hoist { get; set; }
    /// <summary>
    /// Ranking of this role
    /// </summary>
    [JsonPropertyName("rank")]
    public long Rank { get; set; }
}