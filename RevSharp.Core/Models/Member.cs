using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class Member : Clientable
{
    [JsonPropertyName("_id")]
    public MemberId Id { get; set; }
    [JsonPropertyName("joined_at")]
    public string JoinedAt { get; set; }
    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }
    [JsonPropertyName("avatar")]
    public File? Avatar { get; set; }
    [JsonPropertyName("roles")]
    public string[] RoleIds { get; set; }
    
    [JsonPropertyName("timeout")]
    public DateTimeOffset? TimeoutTimestamp { get; set; }

    public async Task<Dictionary<string, ServerRole>?> FetchRoles(Client client)
    {
        var server = await client.GetServer(Id.ServerId);
        if (server == null)
            return null;

        var items = new Dictionary<string, ServerRole>();
        foreach (var item in server.Roles)
        {
            if (RoleIds.Contains(item.Key))
                items.Add(item.Key, item.Value);
        }

        return items;
    }

    public async Task<List<(string, ServerRole)>?> FetchOrderedRoles(Client client)
    {
        var items = await FetchRoles(client);
        if (items == null)
            return null;
        var list = items.Select((v) =>
        {
            return (v.Key, v.Value);
        }).OrderBy(v => v.Value.Rank);
        return list.ToList();
    }
    
    public Member()
    {
        Id = new MemberId();
        RoleIds = Array.Empty<string>();
    }

    public async Task<bool> Fetch(Client client)
    {
        var response = await client.GetAsync($"/servers/{Id.ServerId}/members/{Id.UserId}");
        if (response.StatusCode != HttpStatusCode.OK)
            return false;

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<Member>(stringContent, Client.SerializerOptions);
        Insert(data, this);
        return true;
    }
    public Task<bool> Fetch()
        => Fetch(Client);

    internal static void Insert(Member source, Member target)
    {
        target.Id = source.Id;
        target.JoinedAt = source.JoinedAt;
        target.Nickname = source.Nickname;
        target.Avatar = source.Avatar;
        target.RoleIds = source.RoleIds;
        target.TimeoutTimestamp = source.TimeoutTimestamp;
    }

    public async Task<bool> HasPermission(Client client)
    {
        var server = await client.GetServer(Id.ServerId);
        ServerRole? highestRole = null;
        var sortedRoles = server.Roles
            .OrderBy(v => v.Value.Rank);
        foreach (var item in sortedRoles)
        {
            if (!RoleIds.Contains(item.Key))
                continue;
            
            
        }

        return false;
    }

}

public class MemberId
{
    [JsonPropertyName("server")]
    public string ServerId { get; set; }
    [JsonPropertyName("user")]
    public string UserId { get; set; }
}