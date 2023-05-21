using System.Text.Json;
using RevSharp.Core.Helpers;
using RevSharp.Core.Models.WebSocket;

namespace RevSharp.Core;

internal partial class WebsocketClient
{
    private async Task ParseMessage_Channel(string content, string type)
    {
        switch (type)
        {
            case "ChannelCreate":
                var channelData = ChannelHelper.ParseChannel(content);
                if (channelData != null)
                {
                    channelData.Client = _client;
                    if (_client.AddToCache(channelData))
                        await _client.ChannelCache[channelData.Id].Fetch();
                    _client.OnChannelCreated(_client.ChannelCache[channelData.Id]);
                }
                break;
            case "ChannelDelete":
                var channelDeleteData = JsonSerializer.Deserialize<IdEvent>(
                    content,
                    Client.SerializerOptions);
                if (channelDeleteData != null)
                {
                    _client.OnChannelDeleted(channelDeleteData.Id);
                }
                break;
            case "ChannelStartTyping":
                var channelStartTypingData =
                    JsonSerializer.Deserialize<ChannelTypingEvent>(
                        content,
                        Client.SerializerOptions);

                if (channelStartTypingData != null)
                {
                    _client.OnChannelStartTyping(
                        channelStartTypingData.ChannelId,
                        channelStartTypingData.UserId);
                }
                break;
            case "ChannelStopTyping":
                var channelStopTypingData =
                    JsonSerializer.Deserialize<ChannelTypingEvent>(
                        content,
                        Client.SerializerOptions);

                if (channelStopTypingData != null)
                {
                    _client.OnChannelStopTyping(
                        channelStopTypingData.ChannelId,
                        channelStopTypingData.UserId);
                }
                break;
        }
    }
}