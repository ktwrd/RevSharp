using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using RevSharp.Core.Helpers;

namespace RevSharp.Core.Models;

/// <summary>
/// Server Member
/// </summary>
public class Member : Clientable
{
    /// <summary>
    /// Unique member id
    /// </summary>
    [JsonPropertyName("_id")]
    public MemberId Id { get; set; }
    /// <summary>
    /// Time at which this user joined the server
    /// </summary>
    [JsonPropertyName("joined_at")]
    public string JoinedAt { get; set; }
    
    /// <summary>
    /// Member's nickname
    /// </summary>
    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }
    /// <summary>
    /// Avatar attachment
    /// </summary>
    [JsonPropertyName("avatar")]
    public File? Avatar { get; set; }
    
    /// <summary>
    /// Member's roles
    /// </summary>
    [JsonPropertyName("roles")]
    public string[] RoleIds { get; set; }
    /// <summary>
    /// Timestamp this member is timed out until
    /// </summary>
    [JsonPropertyName("timeout")]
    public DateTimeOffset? TimeoutTimestamp { get; set; }

    /// <summary>
    /// Fetch roles for this member from the API
    /// </summary>
    /// <param name="client"></param>
    /// <param name="forceUpdate">When `true`, bypass the cache when fetching roles</param>
    public async Task<Dictionary<string, ServerRole>?> FetchRoles(Client client, bool forceUpdate = true)
    {
        var server = await client.GetServer(Id.ServerId, forceUpdate: forceUpdate);
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

    /// <summary>
    /// Fetch member roles but ordered based on the <see cref="ServerRole.Rank"/> property
    /// </summary>
    /// <param name="client"></param>
    /// <param name="forceUpdate">When `true`, bypass the cache when fetching roles</param>
    /// <returns></returns>
    public async Task<List<(string, ServerRole)>?> FetchOrderedRoles(Client client, bool forceUpdate = true)
    {
        var items = await FetchRoles(client, forceUpdate: forceUpdate);
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

    /// <summary>
    /// Fetch fresh info from the API and inject it into this instance.
    /// </summary>
    /// <param name="client"></param>
    /// <returns>Did it successfully fetch from the API and inject into this instance</returns>
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

    /// <summary>
    /// Inject data from one member into another. Used when updating self from a fresh copy from the API.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    internal static void Insert(Member source, Member target)
    {
        target.Id = source.Id;
        target.JoinedAt = source.JoinedAt;
        target.Nickname = source.Nickname;
        target.Avatar = source.Avatar;
        target.RoleIds = source.RoleIds;
        target.TimeoutTimestamp = source.TimeoutTimestamp;
    }

    /// <summary>
    /// Does this member have a certain permission. This only checks for the ServerId defined at <see cref="Id.Server"/>
    /// </summary>
    public async Task<bool> HasPermission(Client client, PermissionFlag flag, bool forceUpdate = true)
    {
        var server = await client.GetServer(Id.ServerId, forceUpdate: forceUpdate);
        var user = await client.GetUser(Id.UserId, forceUpdate: forceUpdate);
        var data = await client.CalculatePermissions(user, server);

        return PermissionHelper.HasFlag(data, (int)flag);
    }

    /// <summary>
    /// Does this member have a certain permission. This only checks for the ServerId defined at <see cref="Id.Server"/>
    /// </summary>
    public Task<bool> HasPermission(PermissionFlag flag, bool forceUpdate = true)
        => HasPermission(Client, flag, forceUpdate);

}

/// <summary>
/// Composite primary key consisting of server and user id
/// </summary>
public class MemberId
{
    /// <summary>
    /// Server Id
    /// </summary>
    [JsonPropertyName("server")]
    public string ServerId { get; set; }
    /// <summary>
    /// User Id
    /// </summary>
    [JsonPropertyName("user")]
    public string UserId { get; set; }
}