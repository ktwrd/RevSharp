using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public partial class User : Clientable, /*IUser,*/ ISnowflake, IFetchable
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

    public long Permission
    {
        get
        {
            var permissions = 0L;
            if (Relationship is UserRelationship.Friend or UserRelationship.User)
            {
                return long.MaxValue;
            }

            if (Relationship is UserRelationship.BlockedOther or UserRelationship.BlockedOther)
            {
                return (long)UserPermission.Access;
            }

            if (Relationship is UserRelationship.Incoming or UserRelationship.Outgoing)
            {
                permissions = (long)UserPermission.Access;
            }

            var opt = Client.ChannelCache.Where((v) =>
            {
                if (v.Value.ChannelType == "Group")
                {
                    var groupChannel = (GroupChannel)v.Value;
                    if (groupChannel.RecipientIds.Contains(Id))
                        return true;
                }
                else if (v.Value.ChannelType == "DirectMessage")
                {
                    var dmChannel = (DirectMessageChannel)v.Value;
                    if (dmChannel.RecipientIds.Contains(Id))
                        return true;
                }

                return false;
            });
            var altOpt = Client.ServerCache.Where((v) =>
            {
                return v.Value.Members.Any(m => m.Id.UserId == Id);
            });
            if (opt.Any() || altOpt.Any())
            {
                permissions |= (long)UserPermission.SendMessage;
            }

            return permissions;
        }
    }

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
}