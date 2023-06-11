using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using kate.shared.Helpers;
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
    internal static JsonSerializerOptions SerializerOptions
    {
        get
        {
            var options = new JsonSerializerOptions()
            {
                IgnoreReadOnlyFields = true,
                IgnoreReadOnlyProperties = true,
                IncludeFields = true,
                #if DEBUG
                // WriteIndented = true
                #endif
            };

            options.Converters.Add(new JsonStringEnumConverter());
            
            return options;
        }
    }
    
    public BotController Bot { get; private set; }
    
    #region Constructors
	/// <summary>
	/// Create instance of Client without a token set. You can change the token with <see cref="SetCredentials(string,bool)"/>, but only before <see cref="LoginAsync"/> is called.
	/// </summary>
    public Client()
		: this("", false)
	{
	}
    /// <summary>
    /// Create an instance of the Client
    /// </summary>
    /// <param name="token">Token to use for authentication</param>
    /// <param name="isBot">Does this token belong to a bot. This is important because for bots a different header is sent for HTTP requests.</param>
    public Client(string token, bool isBot)
    {
        Token = token;
        TokenIsBot = isBot;
        Log.CensorList.Add(Token);
        Endpoint = DefaultEndpoint;
        WSClient = new WebsocketClient(this);
        HttpClient = new HttpClient();
        HttpClient.DefaultRequestHeaders.Accept.Clear();
        HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        Bot = new BotController(this);
        ServerCache = new Dictionary<string, Server>();
        UserCache = new Dictionary<string, User>();
        ChannelCache = new Dictionary<string, BaseChannel>();
        MessageCache = new Dictionary<string, Message>();

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

}