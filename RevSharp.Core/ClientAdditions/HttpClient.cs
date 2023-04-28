﻿using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RevSharp.Core.Models;

namespace RevSharp.Core;

public partial class Client
{
    internal HttpClient HttpClient { get; set; }
    
    /// <summary>
    /// Savely throw exception if response is 400. Type should always <see cref="BaseTypedResponse"/>
    /// </summary>
    /// <param name="response">Response from Http request</param>
    /// <exception cref="Exception">Value of <see cref="BaseTypedResponse.Type"/> when Status Code is 400 and the body deserialized successfully</exception>
    private void CheckResponseError(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var stringContent = response.Content.ReadAsStringAsync().Result;
            var data = JsonSerializer.Deserialize<BaseTypedResponse>(stringContent, SerializerOptions);
            if (data == null)
                return;
            throw new Exception($"Bad Request, {data.Type}");
        }
    }
    
    #region HttpClient Wrappers
    internal async Task<HttpResponseMessage> GetAsync(string url)
    {
        var response = await HttpClient.GetAsync($"{Endpoint}{url}");
        CheckResponseError(response);
        return response;
    }
    internal async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        var response = await HttpClient.DeleteAsync(url);
        CheckResponseError(response);
        return response;
    }

    #region Patch
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
    #endregion
    #region Put
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
    #endregion
    #region Post
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
    #endregion
    
    #endregion
    
    
}