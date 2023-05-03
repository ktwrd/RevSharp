using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using RevSharp.Core.Controllers;
using RevSharp.Core.Helpers;
using RevSharp.Core.Models;

namespace RevSharp.Core;

public partial class Client
{
    internal string Token {get; private set; }
    internal bool TokenIsBot { get; private set; }
    internal WebsocketClient WSClient { get; set; }
    /// <summary>
    /// Revolt endpoint to hit, Default: <see cref="DefaultEndpoint"/>
    /// </summary>
    public string Endpoint { get; internal set; }

    public const string DefaultEndpoint = "https://api.revolt.chat";
    internal RevoltNodeResponse? EndpointNodeInfo { get; private set; }

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
                WriteIndented = true
                #endif
            };

            options.Converters.Add(new JsonStringEnumConverter());
            
            return options;
        }
    }
    
    public BotController Bot { get; private set; }
    
    #region Constructors
    public Client(string token, bool isBot)
    {
        Token = token;
        TokenIsBot = isBot;
        Endpoint = DefaultEndpoint;
        WSClient = new WebsocketClient(this);
        HttpClient = new HttpClient();
        Bot = new BotController(this);
        ServerCache = new Dictionary<string, Server>();
        UserCache = new Dictionary<string, User>();
        ChannelCache = new Dictionary<string, BaseChannel>();

        WSClient.MessageReceived += (msg) =>
        {
            msg.Client = this;
            MessageReceived?.Invoke(msg);
        };
        WSClient.ReadyReceived += (message, json) =>
        {
            Ready?.Invoke();
            AddUsersToCache(message.Users);
            AddServersToCache(message.Servers);
        };
        WSClient.AuthenticatedEventReceived += () =>
        {
            ClientAuthenticated?.Invoke();;
        };
        WSClient.ErrorReceived += (e) =>
        {
            if (e != null)
            {
                ErrorReceived?.Invoke(e.Error);
            }
        };
    }
    #endregion
    
    #region Session Management
    public async Task LoginAsync()
    {
        if (!await FetchNodeDetails(Endpoint))
        {
            throw new Exception("Failed to fetch node details");
        }
        Log.WriteLine("Attempting Login");
        HttpClient.DefaultRequestHeaders.Remove("x-bot-token");
        HttpClient.DefaultRequestHeaders.Remove("x-session-token");
        if (TokenIsBot)
            HttpClient.DefaultRequestHeaders.Add("x-bot-token", Token);
        else
            HttpClient.DefaultRequestHeaders.Add("x-session-token", Token);

        Log.WriteLine("Connecting to Bonfire");
        await WSClient.Connect();
        Log.WriteLine("Authenticating with Bonfire");
        await WSClient.Authenticate();

        await FetchCurrentUser();
    }
    public async Task DisconnectAsync()
    {
        Log.WriteLine("Disconnecting from Bonfire");
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
        Log.WriteLine("Setting endpoint");
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
                Log.WriteLine("Successfully fetched node details");
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