namespace RevSharp.Core.Models;

public partial class User
{
    public Task<bool> Unblock(Client client)
        => SetBlockState(client, false);

    public Task<bool> Unblock()
        => Unblock(Client);
}