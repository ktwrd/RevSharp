using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class SystemMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
}

public class SystemTextMessage : SystemMessage
{
    [JsonPropertyName("content")]
    public string Content { get; set; }
}

public class SystemUserAddMessage : SystemMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("by")]
    public string By { get; set; }
}
public class SystemUserRemoveMessage : SystemMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
}

public class SystemUserJoinMessage : SystemMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
}
public class SystemUserLeftMessage : SystemMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
}
public class SystemUserKickedMessage : SystemMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
}
public class SystemUserBannedMessage : SystemMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
}

public class SystemChannelRenamedMessage : SystemMessage
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("by")]
    public string By { get; set; }
}

public class SystemChannelDescriptionRenamedMessage : SystemMessage
{
    [JsonPropertyName("by")]
    public string By { get; set; }
}
public class SystemChannelIconChangedMessage : SystemMessage
{
    [JsonPropertyName("by")]
    public string By { get; set; }
}
public class SystemChannelOwnershipChangedMessage : SystemMessage
{
    [JsonPropertyName("from")]
    public string From { get; set; }
    [JsonPropertyName("to")]
    public string To { get; set; }
}
public enum SystemMessageType
{
    [EnumMember(Value = "text")]
    Text,
    [EnumMember(Value = "user_added")]
    UserAdded,
    [EnumMember(Value = "user_joined")]
    UserJoined,
    [EnumMember(Value = "user_left")]
    UserLeft,
    [EnumMember(Value = "user_kicked")]
    UserKicked,
    [EnumMember(Value = "channel_renamed")]
    ChannelRenamed,
    [EnumMember(Value = "channel_description_changed")]
    ChannelDescriptionChanged,
    [EnumMember(Value = "channel_icon_changed")]
    ChannelIconChanged,
    [EnumMember(Value = "channel_ownership_changed")]
    ChannelOwnershipChanged,
}