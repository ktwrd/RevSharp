using System.Net;

namespace RevSharp.Core.Models;

public partial class Server
{
    public async Task<bool> KickMember(Client client, string userId)
    {
        var response = await client.DeleteAsync($"/servers/{Id}/members/{userId}");
        return response.StatusCode == HttpStatusCode.NoContent;
    }

    public Task<bool> KickMember(string userId)
        => KickMember(Client, userId);

}