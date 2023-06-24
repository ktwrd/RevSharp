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
using StorageObject = Google.Apis.Storage.v1.Data.Object;
namespace RevSharp.Xenia.Modules;

public partial class GoogleApiController
{
    public GoogleCredential? CredentialVision { get; private set; }
    public async Task<GoogleCredential?> GetVisionCredential()
    {
        return CredentialVision ??= await ParseCredential(Reflection.Config.GoogleCloud.DefaultCred);
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
    public async Task<SafeSearchAnnotation?> PerformSafeSearch(StorageObject obj)
    {
        var filename = Path.GetFileName(obj.Name);
        
        var existingSummary = GetSummary(url: filename);
        if (existingSummary != null && existingSummary is
            {
                Annotation: not null
            })
            return existingSummary.Annotation;

        var objUrl = $"gs://{obj.Bucket}/{obj.Name}";
        var image = VisionImage.FromUri(objUrl);
        var data = await PerformSafeSearch(image);
        if (data != null)
        {
            _annotationCache.TryAdd(filename, data);
            _annotationCache.TryAdd(objUrl, data);
            await SaveCache();
        }

        return data;
    }
    public async Task<SafeSearchAnnotation?> PerformSafeSearch(string url)
    {
        if (!_hasCacheRead)
            await ReadCache();
        
        if (_annotationCache.TryGetValue(url, out var search))
        {
            return search;
        }
        
        /*var existingSummary = GetSummary(url: url);
        if (existingSummary is
            {
                Annotation: not null
            })
            return existingSummary.Annotation;*/
        
        // when fails, return null
        var uploadResult = await UploadToBucket(url, Reflection.Config.ContentDetectionBucket, ImageContentTypes);
        return await PerformSafeSearch(uploadResult);
    }
    public async Task<SafeSearchAnnotation?> PerformSafeSearch(VisionImage image)
    {
        var client = await GetImageAnnotator();
        if (client == null)
            throw new Exception("ImageAnnotatorClient is null!");

        var data = await client.DetectSafeSearchAsync(image);
        return data;
    }
    public Task<SafeSearchAnnotation?> PerformSafeSearch(RevoltClient revoltClient, RevoltFile file)
    {
        return PerformSafeSearch(file.GetURL(revoltClient));
    }
    #endregion

    public static string[] ImageContentTypes => new[]
    {
        "image/png", "image/jpeg", "image/gif", "image/webp", "image/apng",
    };

    public class SafeSearchCacheSummaryItem
    {
        public string? Url { get; set; }
        public string Hash { get; set; }
        public string ContentType { get; set; }
        public SafeSearchAnnotation? Annotation { get; set; }
    }

    public SafeSearchCacheSummaryItem? GetSummary(string? url = null, string? hash = null)
    {
        if (!(_annotationCache.ContainsKey(hash ?? "") && _annotationCache.ContainsKey(url ?? "")) || !_urlContentHashCache.ContainsKey(url ?? ""))
            return null;
        if (url != null)
        {
            if (_urlContentHashCache.TryGetValue(url, out var h))
            {
                hash = h;
            }
        }


        _annotationCache.TryGetValue(hash ?? url, out var annotation);
        
        return new SafeSearchCacheSummaryItem()
        {
            Url = url,
            Hash = hash,
            ContentType = _safeSearchHashType[hash],
            Annotation = annotation,
        };
    }

    private void AddToCache(string hash, string url, string contentType)
    {
        _safeSearchHashType.TryAdd(hash, contentType);
        _urlContentHashCache.TryAdd(url, hash);
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
        
    public static string ComputeSha256Hash(byte[] bytes)  
    {  
        var crypt = new SHA256Managed();
        string hash = String.Empty;
        byte[] crypto = crypt.ComputeHash(bytes);
        foreach (byte theByte in crypto)
        {
            hash += theByte.ToString("x2");
        }
        return hash;
    } 
}