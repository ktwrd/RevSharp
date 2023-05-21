using RevSharp.Core.Helpers;
using RevSharp.Core.Models;

namespace RevSharp.Core;

public partial class Client
{
    public event ServerDelegate ServerCreated;

    /// <summary>
    /// Add to cache then invoke <see cref="ServerCreated"/>
    /// </summary>
    internal void OnServerCreated(Server server)
    {
        server.Client = this;
        AddToCache(server);
        if (ServerCreated != null)
        {
            ServerCreated?.Invoke(server);
        }
    }
}