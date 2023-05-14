using System.Net;
using RevSharp.Core.Helpers;

namespace RevSharp.Core.Models;

public partial class Server
{
    public async Task<bool> BanMember(Client client, string userId, string? reason = null)
    {
        if (reason is { Length: > 1024 })
            throw new Exception("Parameter \"reason\" must be less than 1024 characters");

        var response = await client.PutAsync(
            $"/servers/{Id}/bans/{userId}",
            new StringContent(reason ?? "null"));
        return response.StatusCode == HttpStatusCode.OK;
    }
    public Task<bool> BanMember(string userId, string? reason = null)
        => BanMember(Client, userId, reason);

    public Task<bool> BanMember(Client client, User user, string? reason = null)
        => BanMember(client, user.Id, reason);
    public Task<bool> BanMember(User user, string? reason = null)
        => BanMember(Client, user.Id, reason);
}