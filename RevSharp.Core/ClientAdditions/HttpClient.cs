using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using RevSharp.Core.Helpers;
using RevSharp.Core.Models;

namespace RevSharp.Core;

public partial class Client
{
    internal HttpClient HttpClient { get; set; }

    internal void InitRateLimit()
    {
        foreach (var item in RateLimitRoutes)
        {
            string name = $"{item.Item1}_{item.Item2}";
            RateLimitDict.Add(name, new SemaphoreSlim(item.Item3));
        }
    }

    internal SemaphoreSlim GetRateLimitName(string url, string? method)
    {
        foreach (var (endpointRegex, itemMethod, itemTimeout) in RateLimitRoutes)
        {
            var regex = new Regex(endpointRegex);
            if (regex.IsMatch(url) && method == (itemMethod ?? method))
            {
                string name = $"{endpointRegex}_{itemMethod}";
                if (RateLimitDict.TryGetValue(name, out var limitName))
                {
                    /*Log.Info($"{method} {url} = {name}");*/
                    return limitName;
                }
            }
        }
        
        /*Log.Info($"{method} {url} = default");*/

        return DefaultRateLimit;
    }
    
    /// <summary>
    /// Savely throw exception if response is 400. Type should always <see cref="BaseTypedResponse"/>
    /// </summary>
    /// <param name="response">Response from Http request</param>
    /// <exception cref="Exception">Value of <see cref="BaseTypedResponse.Type"/> when Status Code is 400 and the body deserialized successfully</exception>
    private void CheckResponseError(HttpResponseMessage response)
    {
        int code = (int)response.StatusCode;
        if (code is >= 400 and < 500)
        {
            var stringContent = response.Content.ReadAsStringAsync().Result;
            int[] descriptive = new int[]
            {
                422
            };
            if (descriptive.Contains(code))
            {
                throw new RevoltDescriptiveException(stringContent);
            }
            if (code == 429)
                throw new RevoltException("TooManyRequests");
            ResponseHelper.ThrowException(stringContent);
        }
    }
    
    #region HttpClient Wrappers
    internal async Task<HttpResponseMessage> GetAsync(string url)
    {
        var s = GetRateLimitName(url, "GET");
        await s.WaitAsync();
        var response = await HttpClient.GetAsync($"{Endpoint}{url}");
        s.Release();
        CheckResponseError(response);
        return response;
    }
    internal async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        var s = GetRateLimitName(url, "DELETE");
        await s.WaitAsync();
        var response = await HttpClient.DeleteAsync($"{Endpoint}{url}");
        s.Release();
        CheckResponseError(response);
        return response;
    }

    #region Patch
    internal async Task<HttpResponseMessage> PatchAsync(string url, HttpContent content)
    {
        var s = GetRateLimitName(url, "PATCH");
        await s.WaitAsync();
        var response = await HttpClient.PatchAsync($"{Endpoint}{url}", content);
        s.Release();
        CheckResponseError(response);
        return response;
    }
    internal async Task<HttpResponseMessage> PatchAsync(string url, Dictionary<string, object> data)
    {
        var s = GetRateLimitName(url, "PATCH");
        await s.WaitAsync();
        var content = JsonContent.Create(data, options: SerializerOptions);
        var response = await HttpClient.PatchAsync($"{Endpoint}{url}", content);
        s.Release();
        CheckResponseError(response);
        return response;
    }
    #endregion
    #region Put
    internal async Task<HttpResponseMessage> PutAsync(string url, HttpContent? content=null)
    {
        var s = GetRateLimitName(url, "PUT");
        await s.WaitAsync();
        content ??= new StringContent("");
        var response = await HttpClient.PutAsync($"{Endpoint}{url}", content);
        s.Release();
        CheckResponseError(response);
        return response;
    }
    internal async Task<HttpResponseMessage> PutAsync(string url, Dictionary<string, object> data)
    {
        var s = GetRateLimitName(url, "PUT");
        await s.WaitAsync();
        var content = JsonContent.Create(data, options: SerializerOptions);
        var response = await HttpClient.PutAsync($"{Endpoint}{url}", content);
        s.Release();
        CheckResponseError(response);
        return response;
    }
    #endregion
    #region Post
    internal async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
    {
        var s = GetRateLimitName(url, "POST");
        await s.WaitAsync();
        var response = await HttpClient.PostAsync($"{Endpoint}{url}", content);
        s.Release();
        CheckResponseError(response);
        return response;
    }
    internal Task<HttpResponseMessage> PostAsync(string url, Dictionary<string, object> data)
    {
        var content = JsonContent.Create(data, options: SerializerOptions);
        return PostAsync(url, content);
    }
    #endregion
    
    #endregion
    
    
}