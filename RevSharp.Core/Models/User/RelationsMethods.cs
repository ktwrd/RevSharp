using System.Net;
using System.Net.Http.Json;

namespace RevSharp.Core.Models;

public partial class User
{
    public async Task<UserMutualResponse?> FetchMutuals(Client client)
    {
        var m = new UserMutualResponse();
        if (await m.Fetch(client, Id))
            return m;
        return null;
    }

    public Task<UserMutualResponse?> FetchMutuals()
        => FetchMutuals(Client);
    
    #region Friend State
    /// <param name="state">True: Accept friend request, False: deny friend request/remove as friend</param>
    /// <returns>Is response code 200</returns>
    internal async Task<bool> SetFriendState(Client client, bool state)
    {
        if (state)
        {
            var response = await client.PutAsync($"/users/{Id}/friend");
            if (response.StatusCode == HttpStatusCode.OK)
                await Fetch(client);
            return response.StatusCode == HttpStatusCode.OK;
        }
        else
        {
            var response = await client.DeleteAsync($"/users/{Id}/friend");
            if (response.StatusCode == HttpStatusCode.OK)
                await Fetch(client);
            return response.StatusCode == HttpStatusCode.OK;
        }
    }

    public Task<bool> DenyFriendRequest(Client client)
        => SetFriendState(client, false);
    public Task<bool> AcceptFriendRequest(Client client)
        => SetFriendState(client, true);
    public Task<bool> RemoveFriend(Client client)
        => SetFriendState(client, false);
    public async Task<bool> SendFriendRequest(Client client)
    {
        var response = await client.PostAsync("/users/friend", JsonContent.Create(new Dictionary<string, string>()
        {
            {"username", Username}
        }));
        if (response.StatusCode == HttpStatusCode.OK)
        {
            await Fetch(client);
        }
        return response.StatusCode == HttpStatusCode.OK;
    }
    #endregion
    
    #region Block/Unblock
    internal async Task<bool> SetBlockState(Client client, bool block)
    {
        if (block)
        {
            var response = await client.PutAsync($"/users/{Id}/block");
            await Fetch(client);
            return response.StatusCode == HttpStatusCode.OK;
        }
        else
        {
            var response = await client.DeleteAsync($"/users/{Id}/block");
            await Fetch(client);
            return response.StatusCode == HttpStatusCode.OK;
        }
    }

    public Task<bool> Block(Client client)
        => SetBlockState(client, true);

    public Task<bool> Block()
        => Block(Client);
    public Task<bool> Unblock(Client client)
        => SetBlockState(client, false);

    public Task<bool> Unblock()
        => Unblock(Client);

    #endregion
}