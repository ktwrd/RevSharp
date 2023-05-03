using System.Text.Json;
using RevSharp.Core.Helpers;

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
        }
    }
}