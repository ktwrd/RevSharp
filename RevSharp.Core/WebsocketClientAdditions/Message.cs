using System.Text.Json;
using RevSharp.Core.Models;
using RevSharp.Core.Models.WebSocket;

namespace RevSharp.Core;

internal partial class WebsocketClient
{
    private async Task ParseMessage_Message(string content, string type)
    {
        switch (type)
        {
            case "Message":
                var messageData = Message.Parse(content);
                if (messageData != null)
                {
                    Log.Verbose("Invoking MessageReceived");
                    messageData.Client = _client;
                    MessageReceived?.Invoke(messageData);
                }
                break;
            case "MessageAppend":
                var messageAppendData = MessageAppendEvent.Parse(content);
                if (messageAppendData != null)
                {
                    if (_client.MessageCache.TryGetValue(messageAppendData.MessageId, out var value))
                    {
                        if (messageAppendData.Append.Embeds != null)
                        {
                            value.Embeds = messageAppendData.Append.Embeds;
                        }
                    }
                    else
                    {
                        _client.GetMessage(messageAppendData.ChannelId, messageAppendData.MessageId).Wait();
                    }
                }
                break;
            case "MessageDelete":
                var deleteData = JsonSerializer.Deserialize<MessageDeletedEvent>(content, Client.SerializerOptions);
                if (deleteData != null)
                {
                    if (_client.ChannelCache.TryGetValue(deleteData.ChannelId, out var value))
                    {
                        value.OnMessageDeleted(deleteData.MessageId);
                    }
                    _client.OnMessageDeleted(deleteData.MessageId, deleteData.ChannelId);
                    _client.MessageCache.Remove(deleteData.MessageId);
                }
                break;
            case "MessageReact":
                var reactData = JsonSerializer.Deserialize<MessageReactedEvent>(content, Client.SerializerOptions);
                if (reactData != null)
                {
                    _client.OnMessageReactAdd(reactData);
                }
                break;
            case "MessageUnreact":
                var unreactData = JsonSerializer.Deserialize<MessageReactedEvent>(content, Client.SerializerOptions);
                if (unreactData != null)
                {
                    _client.OnMessageReactRemove(unreactData);
                }
                break;
        }
    }
}