using Newtonsoft.Json.Linq;
using RevSharp.Core.Models;
using STJson = System.Text.Json.JsonSerializer;

namespace RevSharp.Core.Helpers;

public static class MessageHelper
{
    private static readonly Dictionary<string, Type> SystemMessageTypeMap = new Dictionary<string, Type>()
    {
        {"Text", typeof(SystemTextMessage)},
        {"UserAdded", typeof(SystemUserAddMessage)},
        {"UserRemove", typeof(SystemUserRemoveMessage)},
        {"UserJoined", typeof(SystemUserJoinMessage)},
        {"UserLeft", typeof(SystemUserLeftMessage)},
        {"UserKicked", typeof(SystemUserKickedMessage)},
        {"UserBanned", typeof(SystemUserBannedMessage)},
        {"ChannelRenamed", typeof(SystemChannelRenamedMessage)},
        {"ChannelDescriptionRenamed", typeof(SystemChannelDescriptionRenamedMessage)},
        {"ChannelIconChanged", typeof(SystemChannelIconChangedMessage)},
        {"ChannelOwnershipChanged", typeof(SystemChannelOwnershipChangedMessage)},
    };

    public static SystemMessage? ParseSystemMessage(string json)
    {
        var messageObject = STJson.Deserialize<Message>(json, Client.SerializerOptions);
        if (messageObject == null)
            return null;
        if (messageObject.SystemMessage == null)
            return null;
        var obj = JObject.Parse(json);

        var systemObjectJson = STJson.Serialize(
            obj["system"]?.ToObject<Dictionary<string, object>>(),
            Client.SerializerOptions);
        var systemMessageType = messageObject.SystemMessage.Type;
        
        if (SystemMessageTypeMap.TryGetValue(systemMessageType, out var value))
        {
            var systemMessageData = obj["system"].ToObject(value)
                as SystemMessage;
            return systemMessageData;
        }
        
        return null;
    }

    private static readonly Dictionary<string, Type> EmbedTypeMap = new Dictionary<string, Type>()
    {
        {"Website", typeof(MetadataEmbed)},
        {"Image", typeof(ImageEmbed)},
        {"Video", typeof(VideoEmbed)},
        {"Text", typeof(TextEmbed)}
    };
    public static BaseEmbed[]? ParseMessageEmbeds(string json)
    {
        var messageObject = STJson.Deserialize<Message>(json, Client.SerializerOptions);
        if (messageObject == null)
            return null;

        var obj = JObject.Parse(json);
        var objEmbedArr = obj["embeds"]?.ToArray();
        if (objEmbedArr == null)
            return null;
        var embedList = new List<BaseEmbed>();
        foreach (var item in objEmbedArr)
        {
            var itemObjectType = item["type"]?.ToString() ?? "";
            if (itemObjectType.Length < 1)
                continue;
            if (!EmbedTypeMap.ContainsKey(itemObjectType))
                continue;
            if (item.ToObject(EmbedTypeMap[itemObjectType]) is BaseEmbed typeCasted)
                embedList.Add(typeCasted);
        }

        return embedList.ToArray();
    }

    public static BaseEmbed[]? ParseEmbeds(JToken[]? objEmbedArr)
    {
        if (objEmbedArr == null)
            return null;
        var embedList = new List<BaseEmbed>();
        foreach (var item in objEmbedArr)
        {
            var itemObjectType = item["type"]?.ToString() ?? "";
            if (itemObjectType.Length < 1)
                continue;
            if (!EmbedTypeMap.ContainsKey(itemObjectType))
                continue;
            if (item.ToObject(EmbedTypeMap[itemObjectType]) is BaseEmbed typeCasted)
                embedList.Add(typeCasted);
        }

        return embedList.ToArray();
    }
}