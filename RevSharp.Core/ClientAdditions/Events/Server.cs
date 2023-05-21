using kate.shared.Helpers;
using RevSharp.Core.Helpers;
using RevSharp.Core.Models;

namespace RevSharp.Core;

public partial class Client
{
    public event ServerDelegate ServerCreated;

    /// <summary>
    /// - Set Client property
    /// - Add to cache
    /// - Invoke <see cref="ServerCreated"/>
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
    
    public event ServerIdDelegate ServerDeleted;

    /// <summary>
    /// - When Server exists in cache
    ///     - Invoke <see cref="Server.Deleted"/>
    /// - Invoke <see cref="ServerDeleted"/>
    /// </summary>
    internal void OnServerDeleted(string serverId)
    {
        if (ServerCache.ContainsKey(serverId))
            ServerCache[serverId].OnDeleted();
        ServerDeleted?.Invoke(serverId);
    }

    public event MemberIdDelegate ServerMemberJoined;
    /// <summary>
    /// - When Server exists in cache
    ///     - Invoke <see cref="Server.MemberJoined"/>
    /// - Invoke <see cref="ServerMemberJoined"/>
    /// </summary>
    internal void OnServerMemberJoined(string serverId, string userId)
    {
        if (ServerCache.TryGetValue(serverId, out var value))
            value.OnMemberJoined(userId);
        ServerMemberJoined?.Invoke(serverId, userId);
    }
    
    public event MemberIdDelegate ServerMemberLeft;
    /// <summary>
    /// - When Server exists in cache
    ///     - Invoke <see cref="Server.OnMemberLeft(string)"/>
    /// - Invoke <see cref="ServerMemberLeft"/>
    /// </summary>
    /// <param name="serverId"></param>
    /// <param name="userId"></param>
    internal void OnServerMemberLeft(string serverId, string userId)
    {
        if (ServerCache.ContainsKey(serverId))
            ServerCache[serverId].OnMemberLeft(userId);
        ServerMemberLeft?.Invoke(serverId, userId);
    }
}