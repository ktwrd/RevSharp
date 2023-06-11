namespace RevSharp.Core.Models;

public partial class User
{
    public string? FetchAvatarUrl(Client? client)
    {
        return Avatar.GetURL(client);
    }

    public string? FetchAvatarUrl() => FetchAvatarUrl(Client);
}