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
        }

        return Task.CompletedTask;
    }
}