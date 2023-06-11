using System.Net;
using System.Text.Json;
using RevSharp.Core.Models;

namespace RevSharp.Core.Helpers;

internal static class ChannelPermissionHelper
{
    public static async Task<bool> SetRolePermission(Client client, IServerChannel channel, string roleId, long allow, long deny)
    {
        var pushData = new TextChannel.SetRolePermissionRequest()
        {
            Permissions = new TextChannel.SetRolePermissionRequestPermission
            {
                Allow = allow,
                Deny = deny
            }
        };
        
        var stringContent = JsonSerializer.Serialize(pushData, Client.SerializerOptions);
        var response = await client.PutAsync(
            Client.SEndpoint.ChannelPermissions(channel.Id, roleId),
            new StringContent(stringContent, null, "application/json"));
        if (response.StatusCode != HttpStatusCode.OK)
            return false;

        await channel.Fetch(client);
        return true;
    }
}