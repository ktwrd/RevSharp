using System.Text.Json;
using RevSharp.Core.Models;
using RevSharp.Core.Models.WebSocket;

namespace RevSharp.Core;

internal partial class WebsocketClient
{
    private Task ParseMessage_Message(string content, string type)
    {
        switch (type)
        {
            case "Message":
                var messageData = Message.Parse(content);
                if (messageData != null)
                {
                    Console.WriteLine("Invoking MessageReceived");
                    messageData.Client = _client;
                    MessageReceived?.Invoke(messageData);
                }
                break;
            case "MessageAppend":
                var messageAppendData = MessageAppendEvent.Parse(content);
                if (messageAppendData != null)
                {
                    if (_client.MessageCache.ContainsKey(messageAppendData.MessageId))
                    {
                        if (messageAppendData.Append.Embeds != null)
                        {
                            _client.MessageCache[messageAppendData.MessageId].Embeds = messageAppendData.Append.Embeds;
                        }
                    }
                    else
                    {
                        _client.GetMessage(messageAppendData.ChannelId, messageAppendData.MessageId).Wait();
                    }
                }
                break;
        }

        return Task.CompletedTask;
    }
}