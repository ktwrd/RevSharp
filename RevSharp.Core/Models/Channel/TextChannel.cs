using System.Diagnostics;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

/// <summary>
/// Text channel belonging to a server
/// </summary>
public class TextChannel : MessageableChannel, IFetchable
{
    /// <summary>
    /// Id of the server this channel belongs to
    /// </summary>
    [JsonPropertyName("server")]
    public string ServerId { get; set; }

    /// <summary>
    /// Whether this channel is marked as not safe for work
    /// </summary>
    [JsonPropertyName("nsfw")]
    public bool IsNsfw { get; set; }
    /// <summary>
    /// Default permissions assigned to users in this channel
    /// </summary>
    [JsonPropertyName("default_permissions")]
    public PermissionCompare? DefaultPermissions { get; set; }
    /// <summary>
    /// Permissions assigned based on role to this channel
    /// </summary>
    [JsonPropertyName("role_permissions")]
    public Dictionary<string, PermissionCompare> RolePermissions { get; set; }
    public TextChannel()
        : this(null, "")
    {}
    internal TextChannel(Client? client, string id)
        : base(client, id)
    {
        RolePermissions = new Dictionary<string, PermissionCompare>();
    }
    public override async Task<bool> Fetch(Client client)
    {
        var data = await GetGeneric<TextChannel>(client);
        if (data == null)
            return false;

        ServerId = data.ServerId;
        Description = data.Description;
        Icon = data.Icon;
        LastMessageId = data.LastMessageId;
        IsNsfw = data.IsNsfw;

        return true;
    }

    public class SetRolePermissionRequest
    {
        [JsonPropertyName("permissions")]
        public SetRolePermissionRequestPermission Permissions { get; set; }
    }

    public class SetRolePermissionRequestPermission
    {
        [JsonPropertyName("allow")]
        public long Allow { get; set; }
        [JsonPropertyName("deny")]
        public long Deny { get; set; }
    }
    public async Task<bool> SetRolePermission(Client client, string roleId, long allow, long deny)
    {
        var pushData = new SetRolePermissionRequest()
        {
            Permissions = new SetRolePermissionRequestPermission
            {
                Allow = allow,
                Deny = deny
            }
        };
        var stringContent = JsonSerializer.Serialize(pushData, Client.SerializerOptions);
        var response = await client.PutAsync(
            $"/channels/{Id}/permissions/{roleId}",
            new StringContent(stringContent, null, "application/json"));
        if (response.StatusCode != HttpStatusCode.OK)
            return false;

        await Fetch(client);
        return true;
    }

    public Task<bool> SetRolePermission(string roleId, long allow, long deny) =>
        SetRolePermission(Client, roleId, allow, deny);

    public Task<bool> SetDefaultPermission(Client client, long allow, long deny) =>
        SetRolePermission(client, "default", allow, deny);

    public Task<bool> SetDefaultPermission(long allow, long deny) =>
        SetDefaultPermission(Client, allow, deny);
}