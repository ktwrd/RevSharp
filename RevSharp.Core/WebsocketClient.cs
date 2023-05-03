using System.Net.WebSockets;
using System.Text.Json;
using kate.shared.Helpers;
using RevSharp.Core.Helpers;
using RevSharp.Core.Models;
using RevSharp.Core.Models.WebSocket;
using WSClient = Websocket.Client.WebsocketClient;

namespace RevSharp.Core;

internal delegate void WebSocketMessageDelegate(string content, byte[] binary, WebSocketMessageType messageType);
internal delegate void EventReceivedDelegate(string eventType, string content);
internal class WebsocketClient
{
    private readonly Client _client;
    internal WebsocketClient(Client client)
    {
        _client = client;
        TextMessageReceived += c => ParseMessage(c).Wait();
    }

    #region Events
    internal event StringDelegate TextMessageReceived;
    internal event GenericDelegate<byte[]> BinaryMessageReceived;
    private void OnTextMessageReceived(string content)
    {
        if (TextMessageReceived != null)
        {
            TextMessageReceived?.Invoke(content);
        }
    }

    private void OnBinaryMessageReceived(byte[] data)
    {
        if (BinaryMessageReceived != null)
        {
            BinaryMessageReceived?.Invoke(data);
        }
    }
    #endregion
    internal TimeSpan ReconnectionTimeout = TimeSpan.FromSeconds(30);
    internal bool Connected { get; private set; }
    internal WSClient? WebSocketClient { get; private set; }
    internal async Task Connect()
    {
        Log.Debug("Connecting to Websocket");
        if (_client.EndpointNodeInfo?.WebSocket == null)
            throw new Exception("_client.EndpointNodeInfo.WebSocket is null");
        string url = _client.EndpointNodeInfo?.WebSocket ?? "wss://ws.revolt.chat";
        WebSocketClient = new WSClient(new Uri(url));
        WebSocketClient.ReconnectTimeout = ReconnectionTimeout;
        WebSocketClient.MessageReceived.Subscribe((message) =>
        {
            Log.Debug("------ Received Message" + "\n" + JsonSerializer.Serialize(new Dictionary<string, object>
            {
                {"type", message.MessageType},
                {"text", message.Text},
                {"binary", message.Binary}
            }, Client.SerializerOptions));
            switch (message.MessageType)
            {
                case WebSocketMessageType.Text:
                    OnTextMessageReceived(message.Text);
                    break;
                case WebSocketMessageType.Binary:
                    OnBinaryMessageReceived(message.Binary);
                    break;
            }
        });
        await WebSocketClient.Start();
    }

    internal static Dictionary<string, Type> ResponseTypeMap = new Dictionary<string, Type>()
    {
        { "Authenticated", typeof(BaseTypedResponse) },
        { "Error", typeof(BonfireError) },
        { "Pong", typeof(BonfireGenericData<int>) },
        { "Ready", typeof(ReadyMessage) },
        { "Message", typeof(BonfireMessage) }
    };

    internal event VoidDelegate AuthenticatedEventReceived;
    internal event GenericDelegate<BonfireError?> ErrorReceived;
    internal event GenericDelegate<int> PongReceived;
    internal event ReadyMessageDelegate ReadyReceived;
    internal event MessageDelegate MessageReceived;
    internal event EventReceivedDelegate EventReceived;
    private Task ParseMessage(string content)
    {
        Log.WriteLine("Parsing Message");
        var messageType = GetMessageType(content);
        if (messageType == null)
            return Task.CompletedTask;
        var deser = JsonSerializer.Deserialize<BaseTypedResponse>(content, Client.SerializerOptions);
        if (deser == null)
        {
            Log.Error($"Failed to deserialize\n--------\n{content}\n--------");
            return Task.CompletedTask;
        }
        Log.Debug($"Received message {deser.Type}");
        switch (deser.Type)
        {
            case "Authenticated":
                AuthenticatedEventReceived?.Invoke();
                break;
            case "Error":
                ErrorReceived?.Invoke(JsonSerializer.Deserialize<BonfireError>(content, Client.SerializerOptions));
                break;
            case "Pong":
                var pongData = JsonSerializer.Deserialize<BonfireGenericData<int>>(content, Client.SerializerOptions);
                PongReceived?.Invoke(pongData?.Data ?? 0);
                break;
            case "Ready":
                var readyData = JsonSerializer.Deserialize<ReadyMessage>(content, Client.SerializerOptions);
                if (readyData != null)
                    ReadyReceived?.Invoke(readyData, content);
                break;
            case "Message":
                var messageData = Message.Parse(content);
                if (messageData != null)
                {
                    messageData.Client = _client;
                    MessageReceived?.Invoke(messageData);
                }
                break;
        }
        EventReceived?.Invoke(deser.Type, content);
        return Task.CompletedTask;
    }

    internal Type? GetMessageType(string message)
    {
        var data = JsonSerializer.Deserialize<BaseTypedResponse>(message, Client.SerializerOptions);
        if (data == null)
            return null;
        return ResponseTypeMap.TryGetValue(data.Type, out var type) ? type : null;
    }

    internal async Task Authenticate()
    {
        Log.WriteLine($"Sending auth message");
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