﻿using System.Diagnostics;
using System.Net.WebSockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.Loader;
using System.Text.Json;
using System.Timers;
using kate.shared.Helpers;
using RevSharp.Core.Helpers;
using RevSharp.Core.Models;
using RevSharp.Core.Models.WebSocket;
using Websocket.Client;

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
    internal Websocket.Client.WebsocketClient? WebSocketClient { get; private set; }
    /// <summary>
    /// Create connection to Bonfire server
    /// </summary>
    /// <exception cref="ClientInitializeException">Thrown when <see cref="Client.EndpointNodeInfo.WebSocket"/> is null</exception>
    internal async Task Connect()
    {
        Log.Debug("Connecting to Websocket");
        if (_client.EndpointNodeInfo?.WebSocket == null)
            throw new ClientInitializeException("_client.EndpointNodeInfo.WebSocket is null");
        string url = _client.EndpointNodeInfo?.WebSocket ?? "wss://ws.revolt.chat";
        url += $"?version=1&format=json&token={_client.Token}";

        WebSocketClient = new Websocket.Client.WebsocketClient(new Uri(url));
        WebSocketClient.ReconnectTimeout = ReconnectionTimeout;
        WebSocketClient.MessageReceived
            .ObserveOn(TaskPoolScheduler.Default)
            .Subscribe(HandleWebSocketMessage);
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

            WhenDisconnect?.Invoke();
        });
        WebSocketClient.ReconnectionHappened.Subscribe((info) =>
        {
            if (FeatureFlags.WebsocketDebugLogging)
            {
                Log.Debug($"ReconnectionHappened {info.Type}");
            }
        });
        Log.Info("Starting WS Client");
        new Thread(_ => WebSocketClient.StartOrFail()).Start();

        await Ping();
        Observable.Interval(TimeSpan.FromSeconds(3))
            .Subscribe(_ => Ping().Wait());
    }
    private void HandleWebSocketMessage(ResponseMessage message)
    {
        if (FeatureFlags.WebsocketDebugLogging)
        {
            Log.Debug("------ Received Message" + "\n" + message.Text);
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
    }

    private static Dictionary<string, Type> _responseTypeMap = new Dictionary<string, Type>()
    {
        { "Authenticated", typeof(BaseTypedResponse) },
        { "Error", typeof(BonfireError) },
        { "Pong", typeof(BonfireGenericData<int>) },
        { "Ready", typeof(ReadyMessage) },
        { "Message", typeof(BonfireMessage) },
        { "NotFound", typeof(BaseTypedResponse) },
        { "ChannelCreate", typeof(BaseChannel)},
        {"ChannelDelete", typeof(IdEvent)},
        {"ChannelStartTyping", typeof(ChannelTypingEvent)},
        {"ChannelStopTyping", typeof(ChannelTypingEvent)},
        {"MessageAppend", typeof(MessageAppendEvent)},
        {"MessageDelete", typeof(MessageDeletedEvent)},
        {"MessageReact", typeof(MessageReactedEvent)},
        {"MessageUnreact", typeof(MessageReactedEvent)},
        {"ServerCreate", typeof(Server)},
        {"ServerDelete", typeof(IdEvent)},
        {"ServerMemberJoin", typeof(UserIdEvent)},
        {"ServerMemberLeave", typeof(UserIdEvent)},
        {"ServerRoleDelete", typeof(RoleIdEvent)},
        {"ServerRoleUpdate", typeof(ServerRoleUpdateMessage)},
        {"UserUpdate", typeof(UserUpdateMessage)},
        {"UserRelationship", typeof(UserRelationshipEvent)}
    };

    internal event VoidDelegate AuthenticatedEventReceived;
    internal event GenericDelegate<BonfireError?> ErrorReceived;
    internal event GenericDelegate<int> PongReceived;
    internal event ReadyMessageDelegate ReadyReceived;
    internal event MessageDelegate MessageReceived;
    internal event EventReceivedDelegate EventReceived;
    /// <summary>
    /// Invoked when the Ready message is received.
    /// </summary>
    internal event VoidDelegate WhenConnect;
    /// <summary>
    /// Invoked when <see cref="WebSocket.Client.WebsocketClient.DisconnectionHappened"/> is invoked.
    /// </summary>
    internal event VoidDelegate WhenDisconnect;
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
        else if (deser.Type.StartsWith("User"))
            await ParseMessage_User(content, deser.Type);
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
                CalculatePing(pongData);
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

    private Dictionary<int, long> PongTimestampDict = new Dictionary<int, long>();
    private void CalculatePing(BonfireGenericData<int> data)
    {
        var currentMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var sourceMs = PongTimestampDict[data.Data];
        Latency = currentMs - sourceMs;
    }

    public long Latency { get; private set; } = 0;
    
    public async Task Ping()
    {
        var rand = new Random().Next();
        var ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        PongTimestampDict.TryAdd(rand, ms);
        PongTimestampDict[rand] = ms;
        await SendMessage(new BonfireGenericData<int>()
        {
            Data = rand,
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
        var content = JsonSerializer.Serialize(item, Client.SerializerOptions).Replace("\r\n","\n");
        if (WebSocketClient == null)
            return;
        if (FeatureFlags.WebsocketDebugLogging)
        {
            Log.Debug("------ Sent Message" + "\n" + content);
        }
        WebSocketClient.Send(content);
    }

    internal async Task Disconnect()
    {
        if (WebSocketClient != null)
        {
            await WebSocketClient.Stop(WebSocketCloseStatus.NormalClosure, "Connection Closed");
            //WebSocketClient.Close();
            return;
        }

        throw new Exception("Client not connected");
    }
}