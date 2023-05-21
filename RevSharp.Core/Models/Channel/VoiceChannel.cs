﻿using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class VoiceChannel : NamedChannel, IFetchable
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

    public override async Task<bool> Fetch(Client client)
    {
        var data = await GetGeneric<VoiceChannel>(client);
        if (data == null)
            return false;

        ServerId = data.ServerId;
        Name = data.Name;
        Description = data.Description;
        Icon = data.Icon;
        IsNsfw = data.IsNsfw;

        return true;
    }

    public async Task<bool> SetRolePermission(Client client, string roleId, long allow, long deny)
    {
        var pushData = new
        {
            permission = new
            {
                allow = allow,
                deny = deny
            }
        };
        var response = await client.PutAsync(
            $"/channels/{Id}/permissions/{roleId}",
            JsonContent.Create(pushData, options: Client.SerializerOptions));
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