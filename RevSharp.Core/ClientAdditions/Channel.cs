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
            var c = ChannelCache[SavedMessagesChannelId] as SavedMessagesChannel;
            await c.Fetch();
            return c;
        }
        var response = await GetAsync($"/users/{CurrentUserId}/dm");
        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<SavedMessagesChannel>(stringContent, SerializerOptions);

        if (data == null) return null;
        
        // Make sure the client is us
        data.Client = this;

        // Return null if failed to fetch
        if (!await data.Fetch())
            return null;
        SavedMessagesChannelId = data.Id;
        
        // Add to cache if not in it and
        // return reference from ChannelCache
        if (!inCache)
            ChannelCache.Add(data.Id, data);
        return ChannelCache[data.Id] as SavedMessagesChannel;
    }

    private void WSHandle_ChannelCreate(string json)
    {
        var data = ChannelHelper.ParseChannel(json);
        if (data == null)
            return;
        data.Client = this;
        ChannelCache.TryAdd(data.Id, data);
    }

    public async Task<BaseChannel?> GetChannel(string channelId)
    {
        // todo
    }

    private async void WSHandle_ChannelUpdated(string json)
    {
        var data = ChannelHelper.ParseChannel(json);
        if (data == null)
            return;

        if (!ChannelCache.TryGetValue(data.Id, out var value))
        {
            await GetChannel(data.Id);
        }
        
        if (await value.Fetch(this))
        {
            ChannelUpdated?.Invoke(data, ChannelCache[data.Id]);   
        }

    }

    private void WSHandle_ChannelDelete(string json)
    {
        var data = ChannelHelper.ParseChannel(json);
        if (data == null)
            return;
        ChannelCache.Remove(data.Id);
        ChannelDeleted?.Invoke(data.Id);
    }

    public event GenericDelegate<string> ChannelDeleted;
    public event ChannelUpdateDelegate ChannelUpdated;
}