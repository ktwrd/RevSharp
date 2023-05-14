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
    public Task<bool> KickMember(Client client, User user)
        => KickMember(client, user.Id);
    public Task<bool> KickMember(User user)
        => KickMember(Client, user.Id);
}