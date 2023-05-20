using kate.shared.Helpers;
using RevSharp.Core.Helpers;
using RevSharp.Core.Models;

namespace RevSharp.Core;

public partial class Client
{
    /// <summary>
    /// Key: User Id
    /// Value: Instance of User
    /// </summary>
    internal Dictionary<string, User> UserCache { get; set; }
    public User? CurrentUser =>
        UserCache.TryGetValue(CurrentUserId, out var user) 
            ? user
            : null;
    public string CurrentUserId { get; private set; } = "";

    /// <summary>
    /// Cached permissions per-channel and per-user
    ///
    /// Key: User Id
    /// Value.Key: Channel Id
    /// Value.Value: Bitwise permissions that the user has in that channel
    /// </summary>
    private Dictionary<string, Dictionary<string, long>> CachedPermission_Channel =
        new Dictionary<string, Dictionary<string, long>>();

    /// <summary>
    /// Cached permissions per-server and per-user
    ///
    /// Key: User Id
    /// Value.Key: Server Id
    /// Value.Value: Bitwise permissions that the user has in that server
    /// </summary>
    private Dictionary<string, Dictionary<string, long>> CachedPermission_Server =
        new Dictionary<string, Dictionary<string, long>>();

    /// <summary>
    /// Calculate permissions for specific user in a specific server.
    /// </summary>
    /// <param name="user">Target user for permission calculation</param>
    /// <param name="server">Target server to calculate permissions for</param>
    /// <returns>Bitwise permissions for what the specified user has in the specified server.</returns>
    public async Task<long> CalculatePermissions(User user, Server server)
    {
        CachedPermission_Server.TryAdd(user.Id, new Dictionary<string, long>());
        if (CachedPermission_Server[user.Id].ContainsKey(server.Id))
            return CachedPermission_Server[user.Id][server.Id];
        long permissions = await PermissionHelper.CalculatePermission(this, user, server);
        CachedPermission_Server[user.Id].TryAdd(server.Id, permissions);
        return CachedPermission_Server[user.Id][server.Id];
    }

    /// <summary>
    /// Calculate permissions for the current user in the server specified.
    /// </summary>
    /// <param name="server">Server to calculate the permissions for</param>
    /// <returns>Bitwise permissions for what the current user has for the server specified</returns>
    public Task<long> CalculatePermissions(Server server)
        => CalculatePermissions(CurrentUser, server);

    /// <summary>
    /// Calculate permissions for a specific user in a channel
    /// </summary>
    /// <returns>Bitwise permissions for what the user has.</returns>
    public async Task<long> CalculatePermissions(User user, BaseChannel channel)
    {
        CachedPermission_Channel.TryAdd(user.Id, new Dictionary<string, long>());
        if (CachedPermission_Channel[user.Id].ContainsKey(channel.Id))
            return CachedPermission_Channel[user.Id][channel.Id];
        long permissions = await PermissionHelper.CalculatePermission(this, user, channel);
        CachedPermission_Channel[user.Id].TryAdd(channel.Id, permissions);
        return CachedPermission_Channel[user.Id][channel.Id];
    }

    /// <summary>
    /// Calculate permissions for the current user in the specified channel
    /// </summary>
    /// <param name="channel">Channel to calculate the permissions for.</param>
    /// <returns>Bitwise permissions that the current user has in the channel specified.</returns>
    public Task<long> CalculatePermissions(BaseChannel channel)
        => CalculatePermissions(CurrentUser, channel);
    
    /// <summary>
    /// Update <see cref="CurrentUser"/> with latest details
    /// </summary>
    /// <returns>Was successful with fetching user</returns>
    public async Task<bool> FetchCurrentUser()
    {
        Log.Verbose("Fetching user @me");
        var user = await GetUser("@me");
        if (user != null)
        {
            CurrentUserId = user.Id;
            user.IsCurrentUser = true;
        }
        return user != null;
    }

    /// <summary>
    /// Fetch user from Revolt and add to cache if it's not there already
    /// </summary>
    /// <param name="id">User Id</param>
    /// <param name="forceUpdate">When `true`, the cache will be ignored and it will fetch directly from the API, like if it was never in the cache to start with</param>
    /// <returns>Null if failed to fetch.</returns>
    public async Task<User?> GetUser(string id, bool forceUpdate = true)
    {
        Log.Verbose($"Fetching user {id}");
        var inCache = UserCache.ContainsKey(id);
        
        if (inCache && !forceUpdate)
            return UserCache[id];
        
        // Create new user if doesn't exist in cache
        var user = inCache
            ? UserCache[id]
            : new User(this, id);
        
        // Fetch latest data from revolt, and
        // add to cache if it's not there.
        if (!await user.Fetch()) return null;
        if (!inCache)
            AddToCache(user);
        
        return user;
    }

    /// <returns>Was this user in the cache already</returns>
    internal bool AddToCache(User user)
    {
        if (UserCache.ContainsKey(user.Id))
            return true;
        UserCache.Add(user.Id, user);
        UserCache[user.Id].Client = this;
        return false;
    }

    /// <returns>User Ids that were in the cache already</returns>
    internal string[] InsertIntoCache(User[] users)
    {
        var list = new List<string>();
        foreach (var i in users)
        {
            if (AddToCache(i))
                list.Add(i.Id);
        }

        return list.ToArray();
    }
    
    /// <summary>
    /// Change the username for the current user.
    /// </summary>
    /// <param name="username">New username</param>
    /// <param name="currentPassword">Current password for this account</param>
    /// <returns>Did it successfully change the username for the current account</returns>
    public Task<bool> ChangeUsername(string username, string currentPassword)
    {
        Log.Verbose($"Changing username");
        return UserHelper.ChangeUsername(this, username, currentPassword);
    }
}