using RevSharp.Xenia.Models;

namespace RevSharp.Xenia.Moderation.Models;

public enum ServerLogEvent
{
    MessageDelete,              // MessageDelete
    MessageEdit,                // MessageAppend, MessageUpdate
    MemberJoin,                 // ServerMemberJoin
    MemberLeave,                // ServerMemberLeave
    MemberRoleAdd,              // ServerMemberUpdate->data->Roles
    MemberRoleRemove,           // ServerMemberUpdate->data->Roles
    MemberNicknameChange,       // ServerMemberUpdate->data->Nickname
    MemberBan,                  // ????
    MemberUnban,                // ????
    RoleCreate,                 // ServerRoleUpdate
    RoleUpdate,                 // ServerRoleUpdate
    RoleRemove,                 // ServerRoleRemove
    ChannelCreate,              // ChannelCreate
    ChannelUpdate,              // ChannelUpdate
    ChannelDelete,              // ChannelDelete
    EmojiCreate,                // EmojiCreate
    EmojiDelete,                // EmojiDelete
    MemberDeleted,              // UserPlatformWipe
}
public class ServerLogConfigModel : BaseMongoModel
{
    public const string CollectionName = "modlogserverconfig";
    public string ServerId { get; set; }
    public string DefaultChannelId { get; set; }
    public Dictionary<string, string> OverrideChannelIds { get; set; }

    public string? GetChannelId(ServerLogEvent logEvent, string fallback = null)
    {
        if (OverrideChannelIds.TryGetValue(logEvent.ToString(), out var result))
        {
            return result;
        }

        return fallback;
    }

    public void SetChannelId(ServerLogEvent logEvent, string? channelId = null)
    {
        var key = logEvent.ToString();
        if (channelId == null)
        {
            if (OverrideChannelIds.ContainsKey(key))
            {
                OverrideChannelIds.Remove(key);
            }
        }
        else
        {
            OverrideChannelIds.TryAdd(key, channelId);
            OverrideChannelIds[key] = channelId;
        }
    }

    public ServerLogConfigModel()
    {
        ServerId = "";
        DefaultChannelId = "";
        OverrideChannelIds = new Dictionary<string, string>();
    }
}