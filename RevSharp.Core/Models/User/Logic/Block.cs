namespace RevSharp.Core.Models;

public partial class User
{
    public Task<bool> Block(Client client)
        => SetBlockState(client, true);

    public Task<bool> Block()
        => Block(Client);
}