using RevSharp.Core.Helpers;
using RevSharp.Core.Models;

namespace RevSharp.Core;

public partial class Client
{
    public event ChannelDelegate ChannelCreated;
    /// <summary>
    /// Add to cache then invoke <see cref="ChannelCreated"/>
    /// </summary>
    internal void OnChannelCreated(BaseChannel channel)
    {
        channel.Client = this;
        AddToCache(channel);
        
        ChannelCreated?.Invoke(ChannelCache[channel.Id]);
    }
    
    
    public event GenericDelegate<string> ChannelDeleted;
    /// <summary>
    /// Invoke <see cref="ChannelDeleted"/>
    /// </summary>
    internal void OnChannelDeleted(string channelId)
    {
        if (ChannelCache.ContainsKey(channelId))
            ChannelCache[channelId].OnDeleted();
        ChannelDeleted?.Invoke(channelId);
    }
    
    public event ChannelUpdateDelegate ChannelUpdated;
    /// <summary>
    /// If in cache, fetch, if not then add to cache. Then invoke <see cref="ChannelUpdated"/>
    /// </summary>
    internal void OnChannelUpdated(BaseChannel previous, BaseChannel current)
    {
        current.Client = this;
        if (ChannelCache.TryGetValue(current.Id, out var value))
        {
            value.Fetch().Wait();
        }
        else
        {
            ChannelCache.Add(current.Id, current);
        }
        
        ChannelUpdated?.Invoke(previous, current);
    }

    public event ChannelTypingDelegate ChannelStartTyping;
    /// <summary>
    /// Invoke <see cref="BaseChannel.OnStartTyping(string)"/>, <see cref="User.OnStartTyping(string)"/>
    /// </summary>
    internal void OnChannelStartTyping(string channelId, string userId)
    {
        if (ChannelCache.ContainsKey(channelId))
        {
            ChannelCache[channelId].OnStartTyping(userId);
        }

        if (UserCache.ContainsKey(userId))
        {
            UserCache[userId].OnStartTyping(channelId);
        }
        
        ChannelStartTyping?.Invoke(channelId, userId);
    }
}