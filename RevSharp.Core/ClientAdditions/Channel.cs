﻿using System.Net;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using RevSharp.Core.Helpers;
using RevSharp.Core.Models;

namespace RevSharp.Core;

public partial class Client
{
    internal Dictionary<string, BaseChannel> ChannelCache { get; set; }

    public string SavedMessagesChannelId { get; private set; } = "";

    public SavedMessagesChannel? SavedMessagesChannel
        => ChannelCache.TryGetValue(SavedMessagesChannelId, out var channel)
            ? (SavedMessagesChannel)channel
            : null;

    /// <summary>
    /// Get the SavedMessagesChannel for the current user you're logged in as.
    /// </summary>
    /// <returns>Will return null if you're not logged in or it failed to fetch.</returns>
    public async Task<SavedMessagesChannel?> GetSavedMessagesChannel()
    {
        if (CurrentUser == null)
            return null;

        var inCache = SavedMessagesChannelId.Length > 0 && ChannelCache.ContainsKey(SavedMessagesChannelId);
        if (inCache)
        {
            Log.Verbose($"In cache, fetching and returning.");
            var c = ChannelCache[SavedMessagesChannelId] as SavedMessagesChannel;
            await c.Fetch();
            return c;
        }
        Log.Verbose($"Not in cache, fetching from API");
        var response = await GetAsync($"/users/{CurrentUserId}/dm");
        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<SavedMessagesChannel>(stringContent, SerializerOptions);

        if (data == null) return null;
        Log.Error($"Deserialize Failed");
        
        // Make sure the client is us
        data.Client = this;

        Log.Verbose($"Attempting data.Fetch()");
        // Return null if failed to fetch
        if (!await data.Fetch())
            return null;
        SavedMessagesChannelId = data.Id;
        
        // Add to cache if not in it and
        // return reference from ChannelCache
        if (!inCache)
        {
            Log.Verbose($"Adding to cache");
            ChannelCache.Add(data.Id, data);
        }
        return ChannelCache[data.Id] as SavedMessagesChannel;
    }

    /// <returns>Was this channel in the cache already</returns>
    internal bool AddToCache(BaseChannel channel)
    {
        if (ChannelCache.ContainsKey(channel.Id))
            return true;
        ChannelCache.Add(channel.Id, channel);
        ChannelCache[channel.Id].Client = this;
        return false;
    }

    /// <returns>Channel Ids that were in the cache already</returns>
    internal string[] InsertIntoCache(BaseChannel[] channels)
    {
        var list = new List<string>();
        foreach (var i in channels)
        {
            if (AddToCache(i))
                list.Add(i.Id);
        }

        return list.ToArray();
    }

    /// <summary>
    /// Get a channel. Parsed into the unique types of channel as well.
    /// </summary>
    /// <param name="channelId">Channel Id to get</param>
    /// <param name="forceUpdate">When `true`, the cache will be ignored and it will fetch directly from the API, like if it was never in the cache to start with</param>
    /// <returns></returns>
    public async Task<BaseChannel?> GetChannel(string channelId, bool forceUpdate = false)
    {
        if (ChannelCache.ContainsKey(channelId))
        {
            Log.Verbose($"{channelId} exists in cache. Fetching");
            ChannelCache[channelId].Client = this;
            if (!forceUpdate)
            {
                Log.Verbose("Not forcing fetch. Returning from cache");
                return ChannelCache[channelId];
            }
            if (await ChannelCache[channelId].Fetch(this))
            {
                Log.Verbose($"{channelId} fetch in cache complete. Returning ChannelCache[{channelId}]");
                return ChannelCache[channelId];
            }
            Log.Warn($"{channelId} fetch failed");
            return null;
        }
        var response = await GetAsync($"/channels/{channelId}");
        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        Log.Verbose($"{channelId} not in cache, getting from api");
        var stringContent = response.Content.ReadAsStringAsync().Result;
        var channel = ChannelHelper.ParseChannel(stringContent);
        if (channel == null)
            return null;

        Log.Verbose($"Adding {channelId} to cache");
        channel.Client = this;
        ChannelCache.TryAdd(channel.Id, channel);
        ChannelCache[channel.Id] = channel;
        
        return channel;
    }
}