using System.Net.WebSockets;
using System.Text.Json;
using kate.shared.Helpers;
using RevSharp.Core.Helpers;
using RevSharp.Core.Models.WebSocket;
using WSClient = Websocket.Client.WebsocketClient;

namespace RevSharp.Core;

internal delegate void WebSocketMessageDelegate(string content, byte[] binary, WebSocketMessageType messageType);
internal class WebsocketClient
{
    private readonly Client _client;
    internal WebsocketClient(Client client)
    {
        _client = client;
    }

    #region Events
    internal event StringDelegate TextMessageReceived;
    internal event GenericDelegate<byte[]> BinaryMessageRecieved;
    private void OnTextMessageReceived(string content)
    {
        if (TextMessageReceived != null)
        {
            TextMessageReceived?.Invoke(content);
        }
    }

    private void OnBinaryMessageRecieved(byte[] data)
    {
        if (BinaryMessageRecieved != null)
        {
            BinaryMessageRecieved?.Invoke(data);
        }
    }
    #endregion
    internal TimeSpan ReconnectionTimeout = TimeSpan.FromSeconds(30);
    internal bool Connected { get; private set; }
    internal WSClient? WebSocketClient { get; private set; }
    internal async Task Connect()
    {
        if (_client.EndpointNodeInfo?.WebSocket == null)
            throw new Exception("_client.EndpointNodeInfo.WebSocket is null");
        string url = _client.EndpointNodeInfo?.WebSocket ?? "wss://ws.revolt.chat";
        WebSocketClient = new WSClient(new Uri(url));
        WebSocketClient.ReconnectTimeout = ReconnectionTimeout;
        WebSocketClient.MessageReceived.Subscribe((message) =>
        {
            switch (message.MessageType)
            {
                case WebSocketMessageType.Text:
                    OnTextMessageReceived(message.Text);
                    break;
                case WebSocketMessageType.Binary:
                    OnBinaryMessageRecieved(message.Binary);
                    break;
            }
        });
        await WebSocketClient.Start();
    }

    internal async Task Authenticate()
    {
        await SendMessage(new AuthenticateMessage()
        {
            Token = _client.Token
        });
    }

    internal async Task SendMessage<T>(T item)
    {
        var content = JsonSerializer.Serialize(item, Client.SerializerOptions);
        if (WebSocketClient == null)
            return;
        await WebSocketClient?.SendInstant(content);
    }

    internal async Task Disconnect()
    {
        if (WebSocketClient != null)
        {
            await WebSocketClient.Stop(WebSocketCloseStatus.NormalClosure, "Connection Closed");
            WebSocketClient.Dispose();
            return;
        }

        throw new Exception("Client not connected");
    }
}