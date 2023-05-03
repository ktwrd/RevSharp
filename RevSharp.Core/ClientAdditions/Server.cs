using RevSharp.Core.Models;

namespace RevSharp.Core;

public partial class Client
{
    internal Dictionary<string, Server> ServerCache { get; set; }
    public async Task<Server?> GetServer(string serverId)
    {
        var inCache = ServerCache.ContainsKey(serverId);
        Log.WriteLine($"{serverId} is " + (inCache ? "in" : "not in") + " cache");
        // Use server from cache if it exists
        var server = inCache
            ? ServerCache[serverId]
            : new Server(this, serverId);

        Log.WriteLine($"{serverId} fetching");
        // Fetch latest data, add to cache if not there.
        if (!await server.Fetch()) return null;
        if (!inCache)
        {
            Log.WriteLine($"{serverId} adding to cache");
            ServerCache.Add(serverId, server);
        }
        return ServerCache[serverId];
    }

    /// <returns>Was this server in the cache already</returns>
    internal bool AddServerToCache(Server server)
    {
        if (ServerCache.ContainsKey(server.Id))
            return true;
        ServerCache.Add(server.Id, server);
        server.Client = this;
        return false;
    }

    /// <returns>Sever Ids that were in the cache already</returns>
    internal string[] AddServersToCache(Server[] server)
    {
        List<string> list = new List<string>();
        foreach (var i in server)
            if (AddServerToCache(i))
                list.Add(i.Id);
        return list.ToArray();
    }
}