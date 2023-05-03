using System.Runtime.Serialization;

namespace RevSharp.Core.Models;

public interface IUser : ISnowflake, IFetchable
{
    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; }
    
    /// <summary>
    /// Avatar as file attachment
    /// </summary>
    public IFile<IFileMetadata> Avatar { get; set; }
    
    /// <summary>
    /// Relationship with other users
    /// </summary>
    public IUserRelation[]? Relations { get; set; }
    
    /// <summary>
    /// Bitfield of user badges
    /// </summary>
    public int? Badges { get; set; }
    /// <summary>
    /// User's current status
    /// </summary>
    public IUserStatus Status { get; set; }
    /// <summary>
    /// User's profile page
    /// </summary>
    public IUserProfile Profile { get; set; }
    
    /// <summary>
    /// Enum of user flags
    /// </summary>
    public UserFlags Flags { get; set; }
    /// <summary>
    /// Whether this user is privileged
    /// </summary>
    public bool IsPrivileged { get; set; }
    /// <summary>
    /// Bot information (if this is a bot)
    /// </summary>
    public IUserBotDetails? Bot { get; set; }
    
    /// <summary>
    /// Current session user's relationship with this user
    /// </summary>
    public UserRelationship Relationship { get; set; }
    /// <summary>
    /// Whether this user is currently online
    /// </summary>
    public bool IsOnline { get; set; }
}

public enum Badges
{
    /// <summary>
    /// Revolt Developer
    /// </summary>
    Developer = 1,
    /// <summary>
    /// Helped translate revolt
    /// </summary>
    Translator = 2,
    /// <summary>
    /// Monetarily supported Revolt
    /// </summary>
    Supporter = 4,
    /// <summary>
    /// Responsibly disclosed a security issue
    /// </summary>
    ResponsibleDisclosure = 8,
    /// <summary>
    /// Revolt Founder
    /// </summary>
    Founder = 16,
    /// <summary>
    /// Platform moderator
    /// </summary>
    PlatformModeration = 32,
    /// <summary>
    /// Active monetary supporter
    /// </summary>
    ActiveSupporter = 64,
    /// <summary>
    /// 🦊🦝
    /// </summary>
    Paw = 128,
    /// <summary>
    /// Joined as one of the first 1000 users in 2021
    /// </summary>
    EarlyAdopter = 256,
    /// <summary>
    /// Amogus
    /// </summary>
    ReservedRelevantJokeBadge1 = 512,
    /// <summary>
    /// Low resolution troll face
    /// </summary>
    ReservedRelevantJokeBadge2  = 1024
}
public interface IUserStatus
{
    /// <summary>
    /// Custom status text
    /// </summary>
    public string Text { get; set; }
    /// <summary>
    /// Current presence option
    /// </summary>
    public UserPresence Presence { get; set; }
}
public interface IUserProfile
{
    /// <summary>
    /// Text content on user's profile
    /// </summary>
    public string Content { get; set; }
    public File Background { get; set; }
}
public interface IUserBotDetails
{
    public string OwnerId { get; set; }
    public IUser? Owner { get; }
}

public enum RelationshipStatus
{
    [EnumMember(Value = "None")]
    None,
    [EnumMember(Value = "User")]
    User,
    [EnumMember(Value = "Friend")]
    Friend,
    [EnumMember(Value = "Outgoing")]
    Outgoing,
    [EnumMember(Value = "Incoming")]
    Incoming,
    [EnumMember(Value = "Blocked")]
    Blocked,
    [EnumMember(Value = "BlockedOther")]
    BlockedOther
}
public interface IUserRelation
{
    public string Id { get; set; }
    public RelationshipStatus Status { get; set; }
}