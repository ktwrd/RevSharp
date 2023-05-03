using RevSharp.Core.Helpers;
using RevSharp.Core.Models;

namespace RevSharp.Core;

public partial class Client
{
    internal Dictionary<string, User> UserCache { get; set; }
    public User? CurrentUser =>
        UserCache.TryGetValue(CurrentUserId, out var user) 
            ? user
            : null;
    public string CurrentUserId { get; private set; } = "";
    
    /// <summary>
    /// Update <see cref="CurrentUser"/> with latest details
    /// </summary>
    /// <returns>Was successful with fetching user</returns>
    public async Task<bool> FetchCurrentUser()
    {
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
    /// <returns>Null if failed to fetch.</returns>
    public async Task<User?> GetUser(string id)
    {
        var inCache = UserCache.ContainsKey(id);
        
        // Create new user if doesn't exist in cache
        var user = inCache
            ? UserCache[id]
            : new User(this, id);
        
        // Fetch latest data from revolt, and
        // add to cache if it's not there.
        if (!await user.Fetch()) return null;
        if (!inCache)
            UserCache.Add(user.Id, user);
        
        return user;
    }
    
    public Task<bool> ChangeUsername(string username, string currentPassword)
    {
        return UserHelper.ChangeUsername(this, username, currentPassword);
    }
}