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
    text,
    user_added,
    user_remove,
    user_joined,
    user_left,
    user_kicked,
    channel_renamed,
    channel_description_changed,
    channel_icon_changed,
    channel_ownership_changed
}