using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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
    internal static JsonSerializerOptions SerializerOptions => new JsonSerializerOptions()
    {
        IgnoreReadOnlyFields = true,
        IgnoreReadOnlyProperties = true,
        IncludeFields = true
    };
    
    public BotController Bot { get; private set; }
    
    #region Constructors
    internal Client()
    {
        Token = "";
        TokenIsBot = false;
        Endpoint = DefaultEndpoint;
        WSClient = new WebsocketClient(this);
        HttpClient = new HttpClient();
        Bot = new BotController(this);
        ServerCache = new Dictionary<string, Server>();
        UserCache = new Dictionary<string, User>();
        ChannelCache = new Dictionary<string, BaseChannel>();
    }
    public Client(string token, bool isBot)
        : base()
    {
        Token = token;
        TokenIsBot = isBot;
    }
    #endregion
    
    #region Session Management
    public async Task LoginAsync()
    {
        HttpClient.DefaultRequestHeaders.Remove("x-bot-token");
        HttpClient.DefaultRequestHeaders.Remove("x-session-token");
        if (TokenIsBot)
            HttpClient.DefaultRequestHeaders.Add("x-bot-token", Token);
        else
            HttpClient.DefaultRequestHeaders.Add("x-session-token", Token);

        await WSClient.Connect();
        await WSClient.Authenticate();

        await FetchCurrentUser();
    }
    public async Task DisconnectAsync()
    {
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
        var result = await FetchNodeDetails(endpoint);
        if (result)
            Endpoint = endpoint;
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
                EndpointNodeInfo = deserialized;
                return true;
            }

            return false;
        }
        #if DEBUG
        Trace.WriteLine($"[Client->TestEndpoint] {endpoint} returned {response.StatusCode}\n{stringContent}");
        Debugger.Break();
        #endif
        return false;
    }

}