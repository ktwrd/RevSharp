using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using kate.shared.Helpers;
using RevSharp.Core.Helpers;
using RevSharp.Core.Models.WebSocket;

namespace RevSharp.Core.Models;

/// <summary>
/// User
/// </summary>
public partial class User : Clientable, /*IUser,*/ ISnowflake, IFetchable
{
    /// <summary>
    /// Is this user the current used that we are connected to?
    /// </summary>
    [JsonIgnore]
    public bool IsCurrentUser { get; internal set; }

    /// <summary>
    /// Does <see cref="Client.CurrentUserId"/> match <see cref="Bot.OwnerId"/>. When either are `null`, this will be `false`
    /// </summary>
    [JsonIgnore]
    public bool IsCurrentUserOwner
    {
        get
        {
            if (Client == null || Bot == null)
                return false;

            return Client.CurrentUserId == Bot.OwnerId;
        }
    }

    /// <summary>
    /// Is this user a Bot?
    /// </summary>
    [JsonIgnore]
    public bool IsBot
    {
        get
        {
            if (Bot == null)
                return false;

            return Bot.OwnerId != null && Bot.OwnerId.Length > 3;
        }
    }
    
    /// <summary>
    /// Unique Id
    /// </summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    /// <summary>
    /// Username
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; }
    /// <summary>
    /// Discriminator
    /// </summary>
    [JsonPropertyName("discriminator")]
    public string Discriminator { get; set; }
    /// <summary>
    /// Display Name
    /// </summary>
    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; }
    /// <summary>
    /// Avatar attachment
    /// </summary>
    [JsonPropertyName("avatar")]
    public File? Avatar { get; set; }
    /// <summary>
    /// Relationships with other users
    /// </summary>
    [JsonPropertyName("relations")]
    public UserRelation[]? Relations { get; set; }
    
    /// <summary>
    /// Bitfield of user badges
    /// </summary>
    [JsonPropertyName("badges")]
    public int? Badges { get; set; }
    /// <summary>
    /// User's current status
    /// </summary>
    [JsonPropertyName("status")]
    public UserStatus Status { get; set; }
    /// <summary>
    /// User's profile page
    /// </summary>
    [JsonPropertyName("profile")]
    public UserProfile Profile { get; set; }
    
    /// <summary>
    /// Enum of user flags
    /// </summary>
    [JsonPropertyName("flags")]
    public UserFlags Flags { get; set; }
    /// <summary>
    /// Whether this user is privileged
    /// </summary>
    [JsonPropertyName("privileged")]
    public bool IsPrivileged { get; set; }
    /// <summary>
    /// Bot information. Only set when this user is actually a bot.
    /// </summary>
    [JsonPropertyName("bot")]
    public UserBotDetails? Bot { get; set; }
    
    /// <summary>
    /// Current session user's relationship with this user
    /// </summary>
    [JsonPropertyName("relationship")]
    public UserRelationship Relationship { get; set; }
    /// <summary>
    /// Whether this user is currently online
    /// </summary>
    [JsonPropertyName("online")]
    public bool IsOnline { get; set; }

    /// <summary>
    /// Calculated permission for DM's
    /// </summary>
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
        Discriminator = data.Discriminator;
        DisplayName = data.DisplayName;
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

    /// <summary>
    /// Pull latest data from API
    /// </summary>
    /// <returns>Was fetch successful</returns>
    public Task<bool> Fetch()
        => Fetch(Client);

    /// <summary>
    /// Fetch this user's profile
    /// </summary>
    public async Task<UserProfile?> FetchProfile(Client client)
    {
        var data = await UserProfile.Fetch(client, Id);
        if (data != null)
            Profile = data;
        return data == null ? null : Profile;
    }

    /// <summary>
    /// Fetch this user's profile
    /// </summary>
    public Task<UserProfile?> FetchProfile()
        => FetchProfile(Client);

    public string? GetProfileBackgroundUrl()
    {
        return Profile.Background.GetURL(Client);
    }
    
    /// <summary>
    /// Fetch the <see cref="DirectMessageChannel"/> for this user. This will be <see cref="SavedMessagesChannel"/> if this is the user that we are logged in as.
    /// </summary>
    /// <returns><see cref="SavedMessagesChannel"/> when you are this user, <see cref="DirectMessageChannel"/> when you are not.</returns>
    public async Task<BaseChannel?> FetchDMChannel(Client client)
    {
        var response = await client.GetAsync($"/users/{Id}/dm");
        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        var stringContent = response.Content.ReadAsStringAsync().Result;
        if (Id == client.CurrentUserId)
        {
            return JsonSerializer.Deserialize<SavedMessagesChannel>(stringContent, Client.SerializerOptions);
        }
        else
        {
            return JsonSerializer.Deserialize<DirectMessageChannel>(stringContent, Client.SerializerOptions);
        }
    }

    /// <summary>
    /// Fetch the <see cref="DirectMessageChannel"/> for this user. This will be <see cref="SavedMessagesChannel"/> if this is the user that we are logged in as.
    /// </summary>
    /// <returns><see cref="SavedMessagesChannel"/> when you are this user, <see cref="DirectMessageChannel"/> when you are not.</returns>
    public Task<BaseChannel?> FetchDMChannel()
        => FetchDMChannel(Client);

    /// <summary>
    /// Emitted when this user starts typing in the channel specified.
    /// </summary>
    public event ChannelIdDelegate StartTyping;
    /// <summary>
    /// Invoke <see cref="StartTyping"/>
    /// </summary>
    internal void OnStartTyping(string channelId)
    {
        StartTyping?.Invoke(channelId);
    }

    /// <summary>
    /// Emitted when this user stops typing in the channel specified.
    /// </summary>
    public event ChannelIdDelegate StopTyping;
    /// <summary>
    /// Invoke <see cref="StopTyping"/>
    /// </summary>
    internal void OnStopTyping(string channelId)
    {
        StopTyping?.Invoke(channelId);
    }

    public event VoidDelegate Update;

    internal void OnUpdate(UserUpdateMessage message)
    {
        Inject(message.Data, this);
        if (message.Clear != null)
        {
            foreach (var i in message.Clear)
            {
                switch (i)
                {
                    case "ProfileContent":
                        Profile.Content = "";
                        break;
                    case "ProfileBackground":
                        Profile.Background = null;
                        break;
                    case "StatusText":
                        Status.Text = "";
                        break;
                    case "Avatar":
                        Avatar = null;
                        break;
                }
            }
        }

        Update?.Invoke();
    }

    public event VoidDelegate RelationshipUpdate;

    internal void OnRelationshipUpdate(UserRelationshipEvent message)
    {
        Relationship = message.Status;
        RelationshipUpdate?.Invoke();
    }

    internal static void Inject(PartialUser source, User target)
    {
        if (source.Id != null)
            target.Id = source.Id;
        if (source.Username != null)
            target.Username = source.Username;
        if (source.Discriminator != null)
            target.Discriminator = source.Discriminator;
        if (source.DisplayName != null)
            target.DisplayName = source.DisplayName;
        if (source.Avatar != null)
            target.Avatar = source.Avatar;
        if (source.Relations != null)
            target.Relations = source.Relations;
        if (source.Badges != null)
            target.Badges = source.Badges;
        if (source.Status != null)
            target.Status = source.Status;
        if (source.Profile != null)
            target.Profile = source.Profile;
        if (source.Flags != null)
            target.Flags = (UserFlags)source.Flags;
        if (source.Privileged != null)
            target.IsPrivileged = (bool)source.Privileged;
        if (source.Bot != null)
            target.Bot = source.Bot;
        if (source.Relationship != null)
            target.Relationship = (UserRelationship)source.Relationship;
        if (source.Online != null)
            target.IsOnline = (bool)source.Online;
    }
}