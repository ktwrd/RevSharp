using RevSharp.Core.Helpers;
using RevSharp.Core.Models;
using RevSharp.Core.Models.WebSocket;

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

    internal void OnMessageDeleted(string messageId)
    {
        if (MessageCache.TryGetValue(messageId, out var value))
        {
            OnMessageDeleted(messageId, value.ChannelId);
        }

        OnMessageDeleted(messageId, "");
    }

    /// <summary>
    /// Invoked when a message has a react added
    /// </summary>
    public event MessageReactedDelegate MessageReactAdd;
    /// <summary>
    /// - Call <see cref="GetMessageOrCache(string, string)"/>
    /// - When not null
    ///     - Call <see cref="Message.OnReactAdd(string, string)"/> in <see cref="MessageCache"/>
    ///     - Invoke <see cref="MessageReactAdd"/>
    /// </summary>
    internal async void OnMessageReactAdd(MessageReactedEvent data)
    {
        var m = await GetMessageOrCache(data.ChannelId, data.MessageId);
        if (m != null)
        {
            MessageCache[data.MessageId].OnReactAdd(data.UserId, data.Emoji);
            MessageReactAdd?.Invoke(data.UserId, data.Emoji, data.MessageId);
        }
    }

    /// <summary>
    /// Invoked when a message has a reaction removed
    /// </summary>
    public event MessageReactedDelegate MessageReactRemove;
    /// <summary>
    /// - Call <see cref="GetMessageOrCache(string, string)"/>
    /// - When not null
    ///     - Call <see cref="Message.OnReactRemove(string, string)"/> in <see cref="MessageCache"/>
    ///     - Invoke <see cref="MessageReactRemove"/>
    /// </summary>
    internal async void OnMessageReactRemove(MessageReactedEvent data)
    {
        var m = await GetMessageOrCache(data.UserId, data.Emoji);
        if (m != null)
        {
            MessageCache[data.MessageId].OnReactRemove(data.UserId, data.Emoji);
            MessageReactRemove?.Invoke(data.UserId, data.Emoji, data.MessageId);
            
        }
    }
}