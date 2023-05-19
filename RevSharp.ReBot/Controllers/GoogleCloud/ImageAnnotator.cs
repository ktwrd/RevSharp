using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Vision.V1;
using Grpc.Auth;
using kate.shared.Helpers;
using RevoltFile = RevSharp.Core.Models.File;
using RevoltClient = RevSharp.Core.Client;
using VisionImage = Google.Cloud.Vision.V1.Image;

namespace RevSharp.ReBot.Modules;

public partial class GoogleApiController
{
    public GoogleCredential? CredentialVision { get; private set; }
    public async Task<GoogleCredential?> GetVisionCredential()
    {
        return CredentialVision ??= await ParseCredential(Program.ConfigData.GoogleCloud.DefaultCred);
    }
    
    private ImageAnnotatorClient? _annotatorClient;
    public async Task<ImageAnnotatorClient?> GetImageAnnotator()
    {
        if (_annotatorClient != null) return _annotatorClient;
        
        var builder = new ImageAnnotatorClientBuilder()
        {
            Endpoint = ImageAnnotatorClient.DefaultEndpoint,
            ChannelCredentials = (CredentialVision ?? await GetDefaultCredential()).ToChannelCredentials()
        };
        if (CredentialVision == null)
        {
            Log.Warn("Using GoogleCloud.DefaultCred for ImageAnnotatorClient");
        }

        _annotatorClient = await builder.BuildAsync();

        return _annotatorClient;
    }
    
    

    private bool _hasCacheRead = false;
    
    
    #region PerformSafeSearch
    public async Task<SafeSearchAnnotation?> PerformSafeSearch(string url)
    {
        if (!_hasCacheRead)
            await ReadCache();
        if (_urlContentHashCache.TryGetValue(url, out var urlContentHash))
            if (_annotationCache.TryGetValue(urlContentHash, out var search))
                return search;
        if (!await DownloadToCache(url))
            return null;

        if (_urlContentHashCache.TryGetValue(url, out var hash))
        {
            if (_safeSearchByteHashCache.TryGetValue(hash, out var byteData))
            {
                var image = VisionImage.FromBytes(byteData);
                var data = await PerformSafeSearch(image);

                if (data != null)
                {
                    _annotationCache.TryAdd(hash, data);
                    _annotationCache.TryAdd(url, data);
                }
            }
        }

        return null;
    }
    public async Task<SafeSearchAnnotation?> PerformSafeSearch(VisionImage image)
    {
        var client = await GetImageAnnotator();
        if (client == null)
            throw new Exception("ImageAnnotatorClient is null!");

        var data = await client.DetectSafeSearchAsync(image);
        return data;
    }
    public async Task<SafeSearchAnnotation?> PerformSafeSearch(RevoltClient revoltClient, RevoltFile file)
    {
        var url = $"{revoltClient.EndpointNodeInfo.Features.Autumn.Url}/{file.Tag}/{file.Id}/{file.Filename}";
        if (!_urlContentHashCache.TryGetValue(url, out var urlHash))
            return await PerformSafeSearch(url);

        if (_annotationCache.TryGetValue(urlHash, out var cachedData))
        {
            return cachedData;
        }
        return await PerformSafeSearch(url);
    }
    #endregion
    
    private async Task<bool> DownloadToCache(string url)
    {
        if (!_hasCacheRead)
            await ReadCache();

        if (_urlContentHashCache.ContainsKey(url))
        {
            return true;
        }
        var result = await _httpClient.GetAsync(url);
        if (result.StatusCode != HttpStatusCode.OK)
            return false;
        var validContentType = new string[]
        {
            "image/png",
            "image/jpeg",
            "image/gif",
            "image/webp",
            "image/apng",
        };
        var contentType = result.Content.Headers.ContentType?.ToString();
        if (contentType == null || result.Content.Headers.ContentType == null || !validContentType.Contains(result.Content.Headers.ContentType?.ToString()))
            return false;

        var data = await FetchByteArrayFromResponse(result);

        var contentHash = ComputeSha256Hash(data);
        _safeSearchHashType.TryAdd(contentHash, contentType);
        _safeSearchByteHashCache.TryAdd(contentHash, data);
        _urlContentHashCache.TryAdd(url, contentHash);
        
        await SaveCache();
        
        return true;
    }
    
    public static string ComputeSha256Hash(byte[] bytes)  
    {  
        // Create a SHA256   
        using (SHA256 sha256Hash = SHA256.Create())  
        {    
  
            // Convert byte array to a string   
            StringBuilder builder = new StringBuilder();  
            for (int i = 0; i < bytes.Length; i++)  
            {  
                builder.Append(bytes[i].ToString("x2"));  
            }  
            return builder.ToString();  
        }  
    } 

    private async Task<byte[]> FetchByteArrayFromResponse(HttpResponseMessage response)
    {
        HttpContent c = response.Content;
        long? contentLength = c.Headers.ContentLength;
        using MemoryStream buffer = new MemoryStream((int)contentLength.GetValueOrDefault());

        using Stream responseStream = await c.ReadAsStreamAsync();
        await responseStream.CopyToAsync(buffer);

        return buffer.Length == 0 ? Array.Empty<byte>() : buffer.ToArray();
    }
}