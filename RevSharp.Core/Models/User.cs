using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class User : /*IUser,*/ ISnowflake, IFetchable
{
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
    {}

    public User(string id)
    {
        Id = id;
    }
    
    public static async Task<User?> Get(string id, Client client)
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
}

public class UserProfile : IUserProfile
{
    [JsonPropertyName("content")]
    public string Content { get; set; }
    [JsonPropertyName("background")]
    public File Background { get; set; }
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