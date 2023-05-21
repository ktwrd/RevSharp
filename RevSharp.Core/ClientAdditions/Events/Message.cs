using RevSharp.Core.Helpers;
using RevSharp.Core.Models;

namespace RevSharp.Core;

public partial class Client
{
    public event MessageDelegate MessageReceived;
    /// <summary>
    /// Add to cache then invoke <see cref="MessageReceived"/>
    /// </summary>
    internal void OnMessageReceived(Message message)
    {
        message.Client = this;
        AddToCache(message);
        
        MessageReceived.Invoke(MessageCache[message.Id]);
    }
    
    public event MessageDeleteDelegate MessageDeleted;

    /// <summary>
    /// Invoke <see cref="MessageDeleted"/>, <see cref="Message.Deleted"/>, <see cref="BaseChannel.MessageDeleted"/>
    /// </summary>
    internal void OnMessageDeleted(string messageId, string channelId)
    {
        MessageDeleted?.Invoke(messageId, channelId);
        if (MessageCache.ContainsKey(messageId))
            MessageCache[messageId].OnDeleted();
        if (ChannelCache.ContainsKey(channelId))
            ChannelCache[channelId].OnMessageDeleted(messageId);
    }
}