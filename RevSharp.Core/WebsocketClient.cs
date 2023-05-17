using System.Net.WebSockets;
using System.Text.Json;
using System.Timers;
using kate.shared.Helpers;
using RevSharp.Core.Helpers;
using RevSharp.Core.Models;
using RevSharp.Core.Models.WebSocket;
using WSClient = Websocket.Client.WebsocketClient;

namespace RevSharp.Core;

internal delegate void WebSocketMessageDelegate(string content, byte[] binary, WebSocketMessageType messageType);
internal delegate void EventReceivedDelegate(string eventType, string content);
internal partial class WebsocketClient
{
    private readonly Client _client;
    internal WebsocketClient(Client client)
    {
        _client = client;
        TextMessageReceived += async (c) =>
        {
            await ParseMessage(c);
        };
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
        url += $"?version=1&format=json&token={_client.Token}";
        WebSocketClient = new WSClient(new Uri(url));
        WebSocketClient.ReconnectTimeout = ReconnectionTimeout;
        WebSocketClient.MessageReceived.Subscribe((message) =>
        {
            if (FeatureFlags.WebsocketDebugLogging)
            {
                Log.Debug("------ Received Message" + "\n" + JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    {"type", message.MessageType},
                    {"text", message.Text},
                    {"binary", message.Binary}
                }, Client.SerializerOptions));
            }
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
        WebSocketClient.DisconnectionHappened.Subscribe((info) =>
        {
            Log.Debug($"DisconnectionHappened {info.Type}");
            if (FeatureFlags.WebsocketDebugLogging)
            {
                Console.WriteLine(JsonSerializer.Serialize(new Dictionary<string, object>()
                {
                    {"type", info.Type},
                    {"closeStatus", info.CloseStatus},
                    {"closeStatusDesc", info.CloseStatusDescription},
                    {"subProtocol", info.SubProtocol},
                    {"exception", info.Exception},
                    {"cancelReconnection", info.CancelReconnection},
                    {"cancelClosing", info.CancelClosing}
                }, Client.SerializerOptions));   
            }
        });
        WebSocketClient.ReconnectionHappened.Subscribe((info) =>
        {
            if (FeatureFlags.WebsocketDebugLogging)
            {
                Log.Debug($"ReconnectionHappened {info.Type}");
            }
        });
        Log.Info("Starting WS Client");
        await WebSocketClient.StartOrFail();
        CreatePingTimer();
    }

    private void CreatePingTimer()
    {
        if (_pingTimer != null)
            return;
        _pingTimer = new System.Timers.Timer();
        _pingTimer.Interval = 5000;
        _pingTimer.Elapsed += _pingTimer_Elapsed;
        _pingTimer.Enabled = true;
        _pingTimer.Start();
    }
    private System.Timers.Timer? _pingTimer = null;

    private void _pingTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _pingTimer?.Stop();
        try
        {
            if (WebSocketClient?.IsRunning ?? false)
                Ping().Wait();
        }
        catch (Exception exception)
        {
            Log.Error(exception);
        }
        _pingTimer?.Start();
    }

    private static Dictionary<string, Type> _responseTypeMap = new Dictionary<string, Type>()
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
    private async Task ParseMessage(string content)
    {
        Log.Verbose("Parsing Message");
        var messageType = GetMessageType(content);
        if (messageType == null)
            return;
        var deser = JsonSerializer.Deserialize<BaseTypedResponse>(content, Client.SerializerOptions);
        if (deser == null)
        {
            Log.Error($"Failed to deserialize\n--------\n{content}\n--------");
            return;
        }
        Log.Debug($"Received message {deser.Type}");
        if (deser.Type.StartsWith("Message"))
        {
            await ParseMessage_Message(content, deser.Type);
        }
        else if (deser.Type.StartsWith("Channel"))
            await ParseMessage_Channel(content, deser.Type);
        else if (deser.Type.StartsWith("Server"))
            await ParseMessage_Server(content, deser.Type);
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
                Log.Info("Ready!");
                break;
        }
        EventReceived?.Invoke(deser.Type, content);
    }

    public async Task Ping()
    {
        await SendMessage(new BonfireGenericData<int>()
        {
            Data = 0,
            Type = "Ping"
        });
    }

    internal Type? GetMessageType(string message)
    {
        var data = JsonSerializer.Deserialize<BaseTypedResponse>(message, Client.SerializerOptions);
        if (data == null)
            return null;
        return _responseTypeMap.TryGetValue(data.Type, out var type) ? type : null;
    }

    internal async Task Authenticate()
    {
        Log.Verbose($"Sending auth message");
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