using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RevSharp.Core.Controllers;
using RevSharp.Core.Models;

namespace RevSharp.Core;

public class Client
{
    internal string Token {get; private set; }
    internal bool TokenIsBot { get; private set; }
    internal HttpClient HttpClient;
    internal WebsocketClient WSClient;
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
    }
    public Client(string token, bool isBot)
        : base()
    {
        Token = token;
        TokenIsBot = isBot;
    }
    #endregion
    
    public User CurrentUser { get; private set; }

    /// <summary>
    /// Update <see cref="CurrentUser"/> with latest details
    /// </summary>
    /// <returns>Was successful with fetching user</returns>
    public async Task<bool> FetchCurrentUser()
    {
        var data = await FetchUser("@me");
        if (data == null)
            return false;
        CurrentUser = data;
        return true;
    }
    public async Task<User?> FetchUser(string id)
    {
        var user = new User(id);
        var success = await user.Fetch(this);
        if (!success)
            return null;
        return user;
    }

    public async Task<bool> ChangeUsername(string username, string currentPassword)
    {
        var response = await PatchAsync("/users/@me/username", new Dictionary<string, object>()
        {
            { "username", username },
            { "password", currentPassword }
        });
        if (response.StatusCode != HttpStatusCode.OK)
            return false;

        return await CurrentUser.Fetch(this);
    }
    
    public async Task<SavedMessagesChannel?> FetchSavedMessagesChannel()
    {
        if (CurrentUser == null)
            return null;
        var response = await GetAsync($"/users/{CurrentUser.Id}/dm");
        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        var stringContent = response.Content.ReadAsStringAsync().Result;
        var data = JsonSerializer.Deserialize<SavedMessagesChannel>(stringContent, SerializerOptions);
        if (data != null && await data.Fetch(this) == false)
            return null;
        return data;
    }
    
    #region HttpClient Wrappers

    private void CheckResponseError(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var stringContent = response.Content.ReadAsStringAsync().Result;
            var data = JsonSerializer.Deserialize<BaseWebSocketMessage>(stringContent, SerializerOptions);
            if (data == null)
                return;
            throw new Exception($"Bad Request, {data.Type}");
        }
    }
    internal async Task<HttpResponseMessage> GetAsync(string url)
    {
        var response = await HttpClient.GetAsync($"{Endpoint}{url}");
        CheckResponseError(response);
        return response;
    }

    internal async Task<HttpResponseMessage> PatchAsync(string url, HttpContent content)
    {
        var response = await HttpClient.PatchAsync($"{Endpoint}{url}", content);
        CheckResponseError(response);
        return response;
    }
    internal async Task<HttpResponseMessage> PatchAsync(string url, Dictionary<string, object> data)
    {
        var content = JsonContent.Create(data, options: SerializerOptions);
        var response = await HttpClient.PatchAsync($"{Endpoint}{url}", content);
        CheckResponseError(response);
        return response;
    }
    internal async Task<HttpResponseMessage> PutAsync(string url, HttpContent? content=null)
    {
        content ??= new StringContent("");
        var response = await HttpClient.PutAsync($"{Endpoint}{url}", content);
        CheckResponseError(response);
        return response;
    }
    internal async Task<HttpResponseMessage> PutAsync(string url, Dictionary<string, object> data)
    {
        var content = JsonContent.Create(data, options: SerializerOptions);
        var response = await HttpClient.PutAsync($"{Endpoint}{url}", content);
        CheckResponseError(response);
        return response;
    }

    internal async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
    {
        var response = await HttpClient.PostAsync($"{Endpoint}{url}", content);
        CheckResponseError(response);
        return response;
    }
    internal Task<HttpResponseMessage> PostAsync(string url, Dictionary<string, object> data)
    {
        var content = JsonContent.Create(data, options: SerializerOptions);
        return PostAsync(url, content);
    }

    internal async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        var response = await HttpClient.DeleteAsync(url);
        CheckResponseError(response);
        return response;
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