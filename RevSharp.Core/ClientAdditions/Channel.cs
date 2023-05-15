using System.Net;
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
    
    private void WSHandle_ChannelCreate(string json)
    {
        var data = ChannelHelper.ParseChannel(json);
        if (data == null)
            return;
        Log.Verbose($"{data.Id} Adding to cache");
        data.Client = this;
        ChannelCache.TryAdd(data.Id, data);
    }

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
        ChannelCache.Add(channel.Id, channel);
        
        return channel;
    }

    private async void WSHandle_ChannelUpdated(string json)
    {
        var data = ChannelHelper.ParseChannel(json);
        if (data == null)
            return;

        Log.Verbose($"{data.Id}");
        if (!ChannelCache.TryGetValue(data.Id, out var value))
        {
            Log.Verbose($"Doesn't exist in cache, fetching");
            await GetChannel(data.Id);
        }
        
        Log.Verbose($"Updating channel from cache");
        if (await value.Fetch(this))
        {
            Log.Verbose($"Invoking ChannelUpdated");
            ChannelUpdated?.Invoke(data, ChannelCache[data.Id]);   
        }

    }

    private void WSHandle_ChannelDelete(string json)
    {
        var data = ChannelHelper.ParseChannel(json);
        if (data == null)
            return;
        Log.Verbose($"{data.Id} Invoking ChannelDeleted");
        ChannelCache.Remove(data.Id);
        ChannelDeleted?.Invoke(data.Id);
    }

    public event GenericDelegate<string> ChannelDeleted;
    public event ChannelUpdateDelegate ChannelUpdated;
}