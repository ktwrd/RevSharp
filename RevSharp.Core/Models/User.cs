using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class User : Clientable, /*IUser,*/ ISnowflake, IFetchable
{
    [JsonIgnore]
    public bool IsCurrentUser { get; internal set; }
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    [JsonPropertyName("username")]
    public string Username { get; set; }
    [JsonPropertyName("avatar")]
    public File Avatar { get; set; }
    [JsonPropertyName("relations")]
    public UserRelation[]? Relations { get; set; }
    [JsonPropertyName("badges")]
    public int? Badges { get; set; }
    [JsonPropertyName("status")]
    public UserStatus Status { get; set; }
    [JsonPropertyName("profile")]
    public UserProfile Profile { get; set; }
    [JsonPropertyName("flags")]
    public UserFlags Flags { get; set; }
    [JsonPropertyName("privileged")]
    public bool IsPrivileged { get; set; }
    [JsonPropertyName("bot")]
    public UserBotDetails? Bot { get; set; }
    [JsonPropertyName("relationship")]
    public UserRelationship Relationship { get; set; }
    [JsonPropertyName("online")]
    public bool IsOnline { get; set; }

    public User()
        : this(null, "")
    {}

    public User(string id)
        : this(null, id)
    {
    }

    public User(Client client, string id)
        : base(client)
    {
        Id = id;
        IsCurrentUser = false;
    }
    
    internal static async Task<User?> Get(string id, Client client)
    {
        var response = await client.GetAsync($"/users/{id}");
        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Failed to fetch user {id} (code: {response.StatusCode})");

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<User>(stringContent, Client.SerializerOptions);
        return data;
    }
    /// <summary>
    /// Pull latest data from API
    /// </summary>
    /// <param name="client"></param>
    /// <returns>Was fetch successful</returns>
    public async Task<bool> Fetch(Client client)
    {
        var data = await Get(Id, client);
        if (data == null)
            return false;

        Id = data.Id;
        Username = data.Username;
        Avatar = data.Avatar;
        Relations = data.Relations;
        Badges = data.Badges;
        Status = data.Status;
        Profile = data.Profile;
        Flags = data.Flags;
        IsPrivileged = data.IsPrivileged;
        Bot = data.Bot;
        if (Bot != null)
        {
            Bot.Owner = await Get(Bot.OwnerId, client);
        }
        Relationship = data.Relationship;
        IsOnline = data.IsOnline;
        return true;
    }

    public Task<bool> Fetch()
        => Fetch(Client);

    public async Task<UserProfile?> FetchProfile(Client client)
    {
        var data = await UserProfile.Fetch(client, Id);
        if (data != null)
            Profile = data;
        return data == null ? null : Profile;
    }

    public Task<UserProfile?> FetchProfile()
        => FetchProfile(Client);
    
    public async Task<DirectMessageChannel?> FetchDMChannel(Client client)
    {
        var response = await client.GetAsync($"/users/{Id}/dm");
        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<DirectMessageChannel>(stringContent, Client.SerializerOptions);
        return data;
    }

    public Task<DirectMessageChannel?> FetchDMChannel()
        => FetchDMChannel(Client);

    
    #region Relationships

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

    #endregion
}

public class UserMutualResponse
{
    [JsonPropertyName("users")]
    public string[] UserIds { get; set; }
    [JsonPropertyName("servers")]
    public string[] ServerIds { get; set; }
    
    [JsonIgnore]
    public User[] Users { get; set; }
    [JsonIgnore]
    public Server[] Servers { get; set; }

    public async Task<bool> Fetch(Client client, string userId)
    {
        var response = await client.GetAsync($"/users/{userId}/mutual");
        if (response.StatusCode != HttpStatusCode.OK)
            return false;

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<UserMutualResponse>(stringContent, Client.SerializerOptions);
        if (data == null)
            return false;

        UserIds = data.UserIds;
        ServerIds = data.UserIds;

        var userList = new List<User>();
        foreach (var i in UserIds)
        {
            var u = new User(i);
            if (await u.Fetch(client))
                userList.Add(u);
        }
        Users = userList.ToArray();

        var serverList = new List<Server>();
        foreach (var i in ServerIds)
        {
            var s = new Server(i);
            if (await s.Fetch(client))
                serverList.Add(s);
        }
        Servers = serverList.ToArray();
        
        return true;
    }
}

public class UserProfile : IUserProfile
{
    [JsonPropertyName("content")]
    public string Content { get; set; }
    [JsonPropertyName("background")]
    public File Background { get; set; }

    public static async Task<UserProfile?> Fetch(Client client, string userId)
    {
        var response = await client.GetAsync($"/users/{userId}/profile");
        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<UserProfile>(stringContent, Client.SerializerOptions);
        return data;
    }
}
public class UserRelation : IUserRelation
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    [JsonPropertyName("status")]
    public RelationshipStatus Status { get; set; }
}
public class UserStatus : IUserStatus
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
    [JsonPropertyName("presence")]
    public UserPresence Presence { get; set; }
}
public class UserBotDetails /*: IUserBotDetails*/
{
    [JsonPropertyName("owner")]
    public string OwnerId { get; set; }
    [JsonIgnore]
    public User? Owner { get; internal set; }
}


public enum UserRelationship
{
    None,
    User,
    Friend,
    Outgoing,
    Incoming,
    Blocked,
    BlockedOther
}