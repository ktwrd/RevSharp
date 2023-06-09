﻿using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using kate.shared.Helpers;
using MimeTypes;
using Newtonsoft.Json.Linq;
using RevSharp.Core.Controllers;
using RevSharp.Core.Helpers;
using RevSharp.Core.Models;
using RevSharp.Core.Models.WebSocket;

namespace RevSharp.Core;

/// <summary>
/// Client used for connecting to the Revolt API. This is where you should start.
/// </summary>
public partial class Client
{
    /// <summary>
    /// Token used for authentication
    /// </summary>
    internal string Token {get; private set; }
    /// <summary>
    /// Does the token belong to a bot
    /// </summary>
    internal bool TokenIsBot { get; private set; }
    /// <summary>
    /// WebSocket middleware for communicating with Bonfire
    /// </summary>
    internal WebsocketClient WSClient { get; set; }

    /// <summary>
    /// WebSocket Ping Latency. Measured in milliseconds
    /// </summary>
    public long WSLatency
    {
        get
        {
            if (WSClient != null)
                return WSClient.Latency;

            return -1;
        }
    }
    /// <summary>
    /// Base REST API Endpoint for Revolt
    ///
    /// Default: <see cref="DefaultEndpoint"/>
    /// </summary>
    public string Endpoint { get; internal set; }

    public const string DefaultEndpoint = "https://api.revolt.chat";
    /// <summary>
    /// Information about the current API server that we're connected to.
    ///
    /// Value is set when <see cref="LoginAsync"/> is called.
    /// </summary>
    public RevoltNodeResponse? EndpointNodeInfo { get; private set; }
    /// <summary>
    /// Generated via <see cref="Client.GenerateSerializerOptions(bool, bool)"/> with `put` set to `false` and `indent` set to `false`. For indenting, use <see cref="SerializerOptionsIndent"/>
    /// </summary>
    public static JsonSerializerOptions SerializerOptions
    {
        get
        {
            return GenerateSerializerOptions(put: false, indent: false);
        }
    }
    /// <summary>
    /// <see cref="Client.SerializerOptions"/> with Indenting enabled
    /// </summary>
    public static JsonSerializerOptions SerializerOptionsIndent
    {
        get
        {
            return GenerateSerializerOptions(put: false, indent: true);
        }
    }
    /// <summary>
    /// <see cref="SerializerOptions"/> but non-static
    /// </summary>
    public JsonSerializerOptions SerializerOptionsL => RevSharp.Core.Client.SerializerOptions;
    /// <summary>
    /// <see cref="SerializerOptionsIndent"/> but non-static
    /// </summary>
    public JsonSerializerOptions SerializerOptionsLI => RevSharp.Core.Client.SerializerOptionsIndent;

    /// <summary>
    /// <see cref="GenerateSerializerOptions"/> with `put=true` and indenting disabled. For indenting, use <see cref="PutSerializerOptionsIndent"/>
    /// </summary>
    public static JsonSerializerOptions PutSerializerOptions
    {
        get
        {
            return GenerateSerializerOptions(put: true, indent: false);
        }
    }
    /// <summary>
    /// Same as <see cref="PutSerializerOptions"/> with indenting enabled
    /// </summary>
    public static JsonSerializerOptions PutSerializerOptionsIndent
    {
        get
        {
            return GenerateSerializerOptions(put: true, indent: true);
        }
    }
    /// <summary>
    /// <see cref="PutSerializerOptions"/> but non-static
    /// </summary>
    public JsonSerializerOptions PutSerializerOptionsL => RevSharp.Core.Client.PutSerializerOptions;
    /// <summary>
    /// <see cref="PutSerializerOptionsIndent"/> but non-static
    /// </summary>
    public JsonSerializerOptions PutSerializerOptionsLI => RevSharp.Core.Client.PutSerializerOptionsIndent;

    /// <summary>
    /// Generate JsonSerializerOptions. `IncludeFields` is always true, Ignored everything that is read-only. Also adds support for <see cref="EnumMemberAttribute"/>
    /// </summary>
    /// <param name="put">When `true`, <see cref="JsonSerializerOptions.DefaultIgnoreCondition"/> will be <see cref="JsonIgnoreCondition.WhenWritingNull"/> instead of the default</param>
    /// <param name="indent">When `true`, <see cref="JsonSerializerOptions.WriteIndented"/> will be `true`.</param>
    /// <returns></returns>
    internal static JsonSerializerOptions GenerateSerializerOptions(bool put = false, bool indent = false)
    {
        
        var options = new JsonSerializerOptions()
        {
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            IncludeFields = true,
            DefaultIgnoreCondition = put ? JsonIgnoreCondition.WhenWritingNull : JsonIgnoreCondition.Never,
            WriteIndented = indent
        };
        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new JsonStringEnumMemberConverter());
        return options;
    }

    
    public BotController Bot { get; private set; }
    internal EndpointFactory EndpointFactory { get; private set; }
    internal static EndpointFactory SEndpoint => new EndpointFactory("");
    
    #region Constructors
	/// <summary>
	/// Create instance of Client without a token set. You can change the token with <see cref="SetCredentials(string,bool)"/>, but only before <see cref="LoginAsync"/> is called.
	/// </summary>
    public Client()
		: this("", false)
	{
	}
    internal SemaphoreSlim Semaphore { get; set; }

    internal List<(string, string?, int)> RateLimitRoutes = new List<(string, string?, int)>()
    {
        (@"\/users", null, 20),
        (@"\/bots", null, 10),
        (@"\/channels", null, 15),
        (@"\/channels\/[a-zA-Z0-9]+\/messages", "POST", 10),
        (@"\/servers", null, 5),
        (@"\/auth", "DELETE", 255),
        (@"\/auth", null, 3),
        (@"\/swagger", null, 100)
    };

    internal Dictionary<string, SemaphoreSlim> RateLimitDict = new Dictionary<string, SemaphoreSlim>();

    internal const int DefaultRateLimitBucketLimit = 20;
    internal SemaphoreSlim DefaultRateLimit = new SemaphoreSlim(DefaultRateLimitBucketLimit, DefaultRateLimitBucketLimit);
    /// <summary>
    /// Create an instance of the Client
    /// </summary>
    /// <param name="token">Token to use for authentication</param>
    /// <param name="isBot">Does this token belong to a bot. This is important because for bots a different header is sent for HTTP requests.</param>
    public Client(string token, bool isBot)
    {
        InitRateLimit();
        Token = token;
        TokenIsBot = isBot;
        Log.CensorList.Add(Token);
        Endpoint = DefaultEndpoint;
        EndpointFactory = new EndpointFactory(this);
        WSClient = new WebsocketClient(this);
        Semaphore = new SemaphoreSlim(10);
        HttpClient = new HttpClient();
        HttpClient.DefaultRequestHeaders.Accept.Clear();
        HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        Bot = new BotController(this);
        ServerCache = new Dictionary<string, Server>();
        UserCache = new Dictionary<string, User>();
        ChannelCache = new Dictionary<string, BaseChannel>();
        MessageCache = new Dictionary<string, Message>();
        MemberCache = new Dictionary<string, Member>();

        WSClient.MessageReceived += (msg) =>
        {
            AddToCache(msg);
            MessageReceived?.Invoke(MessageCache[msg.Id]);
        };
        WSClient.ReadyReceived += (message, json) =>
        {
            Ready?.Invoke();
            InsertIntoCache(message.Users);
            InsertIntoCache(message.Servers);
        };
        WSClient.AuthenticatedEventReceived += () =>
        {
            ClientAuthenticated?.Invoke();
        };
        WSClient.ErrorReceived += (e) =>
        {
            if (e != null)
            {
                ErrorReceived?.Invoke(e.Error);
            }
        };
    }
    /// <summary>
    /// Set credentials. Must be called before <see cref="LoginAsync"/>.
    /// </summary>
    /// <param name="token">Token to authenticate with.</param>
    /// <param name="isBot">Does the token belong to a bot.</param>
    public void SetCredentials(string token, bool isBot)
    {
        Token = token;
        TokenIsBot = isBot;
        Log.CensorList.Add(token);
    }
    /// <summary>
    /// Set credentials and send a customized version of <see cref="AuthenticateMessage"/> when connecting and authenticating with Bonfire (WebSocket server)
    /// </summary>
    /// <param name="authMessage">Customized authenticate message for Bonfire. <see cref="Token"/> will be set from <see cref="AuthenticateMessage.Token"/> field.</param>
    /// <param name="isBot">Does the token belong to a bot.</param>
    public void SetCredentials(AuthenticateMessage authMessage, bool isBot)
    {
        _authMessageContent = authMessage;
        TokenIsBot = isBot;
        Token = authMessage.Token;
        Log.CensorList.Add(authMessage.Token);
    }
    private AuthenticateMessage? _authMessageContent;

    #endregion

    #region Session Management
    private bool IsConnected = false;
    /// <summary>
    /// Login with the given credentials. To set custom credentials after <see cref="Client"/> has been initialized, use <see cref="SetCredentials(string,bool)"/>.
    /// </summary>
    /// <exception cref="ClientInitializeException">
    /// Thrown when <see cref="FetchNodeDetails"/> fails.
    /// </exception>
    public async Task LoginAsync()
    {
        if (!await FetchNodeDetails(Endpoint))
        {
            throw new ClientInitializeException("Failed to fetch node details");
        }
        Log.Info("Attempting Login");
        HttpClient.DefaultRequestHeaders.Remove("x-bot-token");
        HttpClient.DefaultRequestHeaders.Remove("x-session-token");
        if (TokenIsBot)
            HttpClient.DefaultRequestHeaders.Add("x-bot-token", Token);
        else
            HttpClient.DefaultRequestHeaders.Add("x-session-token", Token);

        Log.Info("Connecting to Bonfire");
        await WSClient.Connect();
        Log.Info("Authenticating with Bonfire");
        WSClient.WhenConnect += async () =>
        {
            if (_authMessageContent != null)
            {
                await WSClient.SendMessage(_authMessageContent);
            }
            else
            {
                await WSClient.Authenticate();
            }

            IsConnected = true;
        };
        WSClient.WhenDisconnect += () =>
        {
            IsConnected = false;
        };

        WSClient.AuthenticatedEventReceived += () =>
        {
            FetchCurrentUser().Wait();
        };
    }
    /// <summary>
    /// Close the current websocket connection to Bonfire.
    /// </summary>
    public async Task DisconnectAsync()
    {
        Log.Info("Disconnecting from Bonfire");
        await WSClient.Disconnect();
    }
    #endregion
    
    /// <summary>
    /// Set endpoint to custom one
    /// </summary>
    /// <param name="endpoint">Valid Url for Revolt endpoint</param>
    /// <returns>Is the endpoint valid?</returns>
    public async Task<bool> SetEndpoint(string endpoint)
    {
        Log.Info("Setting endpoint");
        var result = await FetchNodeDetails(endpoint);
        if (result)
            Endpoint = endpoint;
        else
        {
            Log.Error("Failed to fetch node details");
        }
        return result;
    }

    /// <summary>
    /// Fetch details from provided endpoint
    /// </summary>
    /// <param name="endpoint">When null, uses <see cref="Endpoint"/></param>
    /// <returns>Was successful with fetching node details. When true, <see cref="EndpointNodeInfo"/> is set.</returns>
    internal async Task<bool> FetchNodeDetails(string? endpoint)
    {
        var response = await HttpClient.GetAsync(endpoint ?? this.Endpoint);
        var stringContent = response.Content.ReadAsStringAsync().Result;
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var deserialized = JsonSerializer.Deserialize<RevoltNodeResponse>(stringContent, SerializerOptions);
            if (deserialized != null)
            {
                Log.Verbose("Successfully fetched node details");
                EndpointNodeInfo = deserialized;
                return true;
            }

            Log.Error("Failed to deserialize");
            return false;
        }
        #if DEBUG
        Console.WriteLine($"[Client->TestEndpoint] {endpoint} returned {response.StatusCode}\n{stringContent}");
        Debugger.Break();
        #endif
        return false;
    }

    public async Task<string?> UploadFile(Stream stream, string filename, string tag)
    {
        var url = $"{EndpointNodeInfo.Features.Autumn.Url}/{tag}";
        var content = new MultipartFormDataContent
        {
            { new StreamContent(stream), "file", filename }
        };
        var response = await HttpClient.PostAsync(url, content);
        var stringContent = response.Content.ReadAsStringAsync().Result;
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var deser = JsonSerializer.Deserialize<IdEvent>(stringContent, SerializerOptions);

            return deser.Id;
        }

        return null;
    }
    public Task<string?> UploadFile(Stream content, string filename, FileTag tag)
    {
        return UploadFile(content, filename, tag.ToString());
    }
    public Task<string?> UploadFile(string content, string filename, string tag)
    {
        return UploadFile(new MemoryStream(Encoding.UTF8.GetBytes(content)), filename, tag);
    }
    public Task<string?> UploadFile(string content, string filename, FileTag tag)
    {
        return UploadFile(content, filename, tag.ToString());
    }

}
public enum FileTag
{
    [EnumMember(Value = "attachments")]
    Attachment,
    [EnumMember(Value = "avatars")]
    Avatar,
    [EnumMember(Value = "backgrounds")]
    Background,
    [EnumMember(Value = "icons")]
    Icon,
    [EnumMember(Value = "banners")]
    Banner,
    [EnumMember(Value = "emojis")]
    Emoji,
}