﻿using RevSharp.Core.Models;

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
}