namespace RevSharp.Core.Models;

public enum PermissionFlag : long
{
    // * Generic permissions
    /// Manage the channel or channels on the server
    ManageChannel = 1L << 0,
    /// Manage the server
    ManageServer = 1L << 1,
    /// Manage permissions on servers or channels
    ManagePermissions = 1L << 2,
    /// Manage roles on server
    ManageRole = 1L << 3,
    /// Manage server customisation (includes emoji)
    ManageCustomisation = 1L << 4,

    // % 1 bit reserved

    // * Member permissions
    /// Kick other members below their ranking
    KickMembers = 1L << 6,
    /// Ban other members below their ranking
    BanMembers = 1L << 7,
    /// Timeout other members below their ranking
    TimeoutMembers = 1L << 8,
    /// Assign roles to members below their ranking
    AssignRoles = 1L << 9,
    /// Change own nickname
    ChangeNickname = 1L << 10,
    /// Change or remove other's nicknames below their ranking
    ManageNicknames = 1L << 11,
    /// Change own avatar
    ChangeAvatar = 1L << 12,
    /// Remove other's avatars below their ranking
    RemoveAvatars = 1L << 13,

    // % 7 bits reserved

    // * Channel permissions
    /// View a channel
    ViewChannel = 1L << 20,
    /// Read a channel's past message history
    ReadMessageHistory = 1L << 21,
    /// Send a message in a channel
    SendMessage = 1L << 22,
    /// Delete messages in a channel
    ManageMessages = 1L << 23,
    /// Manage webhook entries on a channel
    ManageWebhooks = 1L << 24,
    /// Create invites to this channel
    InviteOthers = 1L << 25,
    /// Send embedded content in this channel
    SendEmbeds = 1L << 26,
    /// Send attachments and media in this channel
    UploadFiles = 1L << 27,
    /// Masquerade messages using custom nickname and avatar
    Masquerade = 1L << 28,
    /// React to messages with emojis
    React = 1L << 29,

    // * Voice permissions
    /// Connect to a voice channel
    Connect = 1L << 30,
    /// Speak in a voice call
    Speak = 1L << 31,
    /// Share video in a voice call
    Video = 1L << 32,
    /// Mute other members with lower ranking in a voice call
    MuteMembers = 1L << 33,
    /// Deafen other members with lower ranking in a voice call
    DeafenMembers = 1L << 34,
    /// Move members between voice channels
    MoveMembers = 1L << 35,

    // * Misc. permissions
    // % Bits 36 to 52: free area
    // % Bits 53 to 64: do not use

    // * Grant all permissions
    /// Safely grant all permissions
    GrantAllSafe = 0x000F_FFFF_FFFF_FFFF,

    /// Grant all permissions
    GrantAll = long.MaxValue,
}