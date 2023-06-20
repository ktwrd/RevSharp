using RevSharp.Core.Models;

namespace RevSharp.Core;

public partial class Client
{
    internal Dictionary<string, Message> MessageCache { get; set; }

    /// <returns>Was this message in the cache already</returns>
    internal bool AddToCache(Message message)
    {
        if (MessageCache.ContainsKey(message.Id))
            return true;
        MessageCache.Add(message.Id, message);
        MessageCache[message.Id].Client = this;
        return false;
    }

    /// <returns>Message Ids that were in the cache already</returns>
    internal string[] InsertIntoCache(Message[] messages)
    {
        var list = new List<string>();
        foreach (var i in messages)
            if (AddToCache(i))
                list.Add(i.Id);
        return list.ToArray();
    }

    /// <summary>
    /// Get a Message from the cache or directly from the API
    /// </summary>
    /// <param name="channelId">Channel Id the message belongs to</param>
    /// <param name="messageId">Message Id to fetch</param>
    /// <returns>Will return `null` if it failed to fetch or something else happened.</returns>
    internal async Task<Message?> GetMessage(string channelId, string messageId)
    {
        if (MessageCache.ContainsKey(messageId))
        {
            MessageCache[messageId].Client = this;
            if (await MessageCache[messageId].Fetch(this))
                return MessageCache[messageId];
        }

        var msg = new Message(this, channelId, messageId);
        msg.Client = this;

        if (!await msg.Fetch())
            return null;

        AddToCache(msg);
        
        return MessageCache[messageId];
    }

    /// <summary>
    /// Return message from cache if it exists there. Otherwise return <see cref="GetMessage"/>
    /// </summary>
    internal async Task<Message?> GetMessageOrCache(string channelId, string messageId)
    {
        if (MessageCache.ContainsKey(messageId))
            return MessageCache[messageId];

        return await GetMessage(channelId, messageId);
    }
}