using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using RevSharp.Xenia;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Controllers;

[RevSharpModule]
public partial class AuthentikController : BaseModule
{
    private HttpClient _http;
    public override async Task Initialize(ReflectionInclude reflection)
    {
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("authorization", $"Bearer {Program.ConfigData.AuthentikToken}");
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Program.ConfigData.AuthentikToken);
    }

    private void ThrowOnFailure(HttpResponseMessage message)
    {
        var failureList = new HttpStatusCode[]
        {
            HttpStatusCode.Forbidden
        };
        if (failureList.Contains(message.StatusCode))
        {
            var stringContent = message.Content.ReadAsStringAsync().Result;
            switch (message.StatusCode)
            {
                case HttpStatusCode.Forbidden:
                    var forbiddenDeser = JsonSerializer.Deserialize<AuthentikGenericAPIError>(
                        stringContent, Program.SerializerOptions);
                    throw new AuthentikException(forbiddenDeser);
                    break;
            }
        }
    }
    
    private HttpRequestMessage GetBaseSend(string url, HttpMethod method, HttpContent? content)
    {
        return new HttpRequestMessage()
        {
            RequestUri = new Uri($"https://{Program.ConfigData.AuthentikUrl}/api/v3/{url}"),
            Headers =
            {
                {
                    "authorization", $"Bearer {Program.ConfigData.AuthentikToken}"
                },
                {
                    "accept", "application/json"
                }
            },
            Method = method,
            Content = content
        };
    }
    
    public async Task<HttpResponseMessage> GetAsync(string url)
    {
        var data = GetBaseSend(url, HttpMethod.Get, null);
        var res = await _http.SendAsync(data);
        ThrowOnFailure(res);
        return res;
    }

    public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
    {
        var data = GetBaseSend(url, HttpMethod.Post, content);
        var res = await _http.SendAsync(data);
        ThrowOnFailure(res);
        return res;
    }

    public async Task<HttpResponseMessage> PutAsync(string url, HttpContent content)
    {
        var data = GetBaseSend(url, HttpMethod.Put, content);
        var res = await _http.SendAsync(data);
        ThrowOnFailure(res);
        return res;
    }

    public async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        var data = GetBaseSend(url, HttpMethod.Delete, null);
        var res = await _http.SendAsync(data);
        ThrowOnFailure(res);
        return res;
    }
}

public class AuthentikGenericAPIError
{
    [JsonPropertyName("detail")]
    public string Detail { get; set; }
    [JsonPropertyName("code")]
    public string? ErrorCode { get; set; }
}