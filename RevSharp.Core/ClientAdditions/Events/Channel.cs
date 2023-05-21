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
}