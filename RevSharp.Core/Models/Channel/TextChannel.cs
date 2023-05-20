using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

/// <summary>
/// Text channel belonging to a server
/// </summary>
public class TextChannel : BaseChannel, IFetchable
{
    /// <summary>
    /// Id of the server this channel belongs to
    /// </summary>
    [JsonPropertyName("server")]
    public string ServerId { get; set; }
    
    /// <summary>
    /// Display name of the channel
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
    /// <summary>
    /// Channel description
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    /// <summary>
    /// Custom icon attachment
    /// </summary>
    [JsonPropertyName("icon")]
    public File? Icon { get; set; }
    /// <summary>
    /// Id of the last message sent in this channel
    /// </summary>
    [JsonPropertyName("last_message_id")]
    public string LastMessageId { get; set; }
    
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