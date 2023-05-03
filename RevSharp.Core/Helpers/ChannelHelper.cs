using RevSharp.Core.Models;
using Newtonsoft.Json.Linq;
using STJson = System.Text.Json.JsonSerializer;

namespace RevSharp.Core.Helpers;

public static class ChannelHelper
{
    private static readonly Dictionary<string, Type> ChannelTypeMap = new Dictionary<string, Type>()
    {
        {"SavedMessages", typeof(SavedMessagesChannel)},
        {"DirectMessage", typeof(DirectMessageChannel)},
        {"Group", typeof(GroupChannel)},
        {"TextChannel", typeof(TextChannel)},
        {"VoiceChannel", typeof(VoiceChannel)}
    };

    public static BaseChannel? ParseChannel(string json)
    {
        var channelObject = STJson.Deserialize<BaseChannel>(json, Client.SerializerOptions);
        if (channelObject == null)
            return null;

        var channelTypeName = channelObject.ChannelType;
        if (ChannelTypeMap.TryGetValue(channelTypeName, out var value))
        {
            return STJson.Deserialize(json, value, Client.SerializerOptions) as BaseChannel;
        }

        return null;
    }
}