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
}