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
            Log.WriteLine($"In cache, fetching and returning.");
            var c = ChannelCache[SavedMessagesChannelId] as SavedMessagesChannel;
            await c.Fetch();
            return c;
        }
        Log.WriteLine($"Not in cache, fetching from API");
        var response = await GetAsync($"/users/{CurrentUserId}/dm");
        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<SavedMessagesChannel>(stringContent, SerializerOptions);

        if (data == null) return null;
        Log.Error($"Deserialize Failed");
        
        // Make sure the client is us
        data.Client = this;

        Log.WriteLine($"Attempting data.Fetch()");
        // Return null if failed to fetch
        if (!await data.Fetch())
            return null;
        SavedMessagesChannelId = data.Id;
        
        // Add to cache if not in it and
        // return reference from ChannelCache
        if (!inCache)
        {
            Log.WriteLine($"Adding to cache");
            ChannelCache.Add(data.Id, data);
        }
        return ChannelCache[data.Id] as SavedMessagesChannel;
    }

    private void WSHandle_ChannelCreate(string json)
    {
        var data = ChannelHelper.ParseChannel(json);
        if (data == null)
            return;
        Log.WriteLine($"{data.Id} Adding to cache");
        data.Client = this;
        ChannelCache.TryAdd(data.Id, data);
    }

    public async Task<BaseChannel?> GetChannel(string channelId)
    {
        if (ChannelCache.ContainsKey(channelId))
        {
            Log.WriteLine($"{channelId} exists in cache. Fetching");
            ChannelCache[channelId].Client = this;
            if (await ChannelCache[channelId].Fetch(this))
            {
                Log.WriteLine($"{channelId} fetch in cache complete. Returning ChannelCache[{channelId}]");
                return ChannelCache[channelId];
            }
            Log.WriteLine($"{channelId} fetch failed");
            return null;
        }
        var response = await HttpClient.GetAsync($"/channels/{channelId}");
        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        Log.WriteLine($"{channelId} not in cache, getting from api");
        var stringContent = response.Content.ReadAsStringAsync().Result;
        var channel = ChannelHelper.ParseChannel(stringContent);
        if (channel == null)
            return null;

        Log.WriteLine($"Adding {channelId} to cache");
        channel.Client = this;
        ChannelCache.Add(channel.Id, channel);
        
        return channel;
    }

    private async void WSHandle_ChannelUpdated(string json)
    {
        var data = ChannelHelper.ParseChannel(json);
        if (data == null)
            return;

        Log.WriteLine($"{data.Id}");
        if (!ChannelCache.TryGetValue(data.Id, out var value))
        {
            Log.WriteLine($"Doesn't exist in cache, fetching");
            await GetChannel(data.Id);
        }
        
        Log.WriteLine($"Updating channel from cache");
        if (await value.Fetch(this))
        {
            Log.WriteLine($"Invoking ChannelUpdated");
            ChannelUpdated?.Invoke(data, ChannelCache[data.Id]);   
        }

    }

    private void WSHandle_ChannelDelete(string json)
    {
        var data = ChannelHelper.ParseChannel(json);
        if (data == null)
            return;
        Log.WriteLine($"{data.Id} Invoking ChannelDeleted");
        ChannelCache.Remove(data.Id);
        ChannelDeleted?.Invoke(data.Id);
    }

    public event GenericDelegate<string> ChannelDeleted;
    public event ChannelUpdateDelegate ChannelUpdated;
}