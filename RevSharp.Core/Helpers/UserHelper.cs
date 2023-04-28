using System.Net;

namespace RevSharp.Core.Helpers;

internal static class UserHelper
{
    internal static async Task<bool> ChangeUsername(Client client, string username, string password)
    {
        var response = await client.PatchAsync("/users/@me/username", new Dictionary<string, object>()
        {
            { "username", username },
            { "password", password }
        });
        if (response.StatusCode != HttpStatusCode.OK)
            return false;

        if (client.CurrentUser != null)
            return await client.CurrentUser.Fetch(client);
        return true;
    }
}