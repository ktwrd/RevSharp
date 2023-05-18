using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Vision.V1;
using Grpc.Auth;
using RevSharp.ReBot.Models;
using RevSharp.ReBot.Reflection;
using RevoltFile = RevSharp.Core.Models.File;
using RevoltClient = RevSharp.Core.Client;
using VisionImage = Google.Cloud.Vision.V1.Image;

namespace RevSharp.ReBot.Modules;

[RevSharpModule]
public class GoogleAPIController : BaseModule
{
    private GoogleCredential _cred_default;
    private GoogleCredential? _cred_vision;
    public override async Task Initialize(ReflectionInclude reflection)
    {
        _cred_default = await ParseCredential(Program.ConfigData.GoogleCloud.DefaultCred);
        if (_cred_default == null)
        {
            Log.Error("_cred_default was parsed to null. Aborting process");
            Program.Quit(1);
        }
        
        _cred_vision = await ParseCredential(Program.ConfigData.GoogleCloud.VisionAPI)
            ?? _cred_vision;
    }

    private ImageAnnotatorClient? _annotatorClient;

    public async Task<ImageAnnotatorClient?> GetImageAnnotator()
    {
        if (_annotatorClient != null) return _annotatorClient;
        
        var builder = new ImageAnnotatorClientBuilder()
        {
            Endpoint = ImageAnnotatorClient.DefaultEndpoint,
            ChannelCredentials = (_cred_vision ?? _cred_default).ToChannelCredentials()
        };
        if (_cred_vision == null)
        {
            Log.Warn("Using GoogleCloud.DefaultCred for ImageAnnotatorClient");
        }

        _annotatorClient = await builder.BuildAsync();

        return _annotatorClient;
    }

    private Dictionary<string, string> SafeSearchCache = new Dictionary<string, string>();
    private Dictionary<string, byte[]> SafeSearchByteCache = new Dictionary<string, byte[]>();
    private Dictionary<string, SafeSearchAnnotation> AnnotationCache = new Dictionary<string, SafeSearchAnnotation>();
    private HttpClient HttpClient = new HttpClient();
    public async Task<SafeSearchAnnotation?> PerformSafeSearch(string url)
    {
        if (AnnotationCache.TryGetValue(url, out var cachedData))
            return cachedData;
        if (!await DownloadToCache(url))
            return null;
        var image = VisionImage.FromBytes(SafeSearchByteCache[url]);
        return await PerformSafeSearch(image);
    }

    private async Task<bool> DownloadToCache(string url)
    {
        if (SafeSearchCache.ContainsKey(url))
            return true;
        var result = await HttpClient.GetAsync(url);
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
        if (!validContentType.Contains(result.Content.Headers.ContentType.ToString()))
            return false;

        var data = await FetchByteArrayFromResponse(result);
        SafeSearchByteCache.TryAdd(url, data);
        SafeSearchByteCache[url] = data;
        var dataBase = Convert.ToBase64String(data);
        var dataStringBuilder = new StringBuilder();
        dataStringBuilder.Append("data:");
        dataStringBuilder.Append(result.Content.Headers.ContentType.ToString().Split(";")[0]);
        dataStringBuilder.Append(";base64");
        dataStringBuilder.Append(",");
        dataStringBuilder.Append(dataBase);

        var dataString = dataStringBuilder.ToString();
        SafeSearchCache.TryAdd(url, dataString);
        SafeSearchCache[url] = dataString;
        return true;
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

    public async Task<SafeSearchAnnotation?> PerformSafeSearch(VisionImage image)
    {
        var client = await GetImageAnnotator();
        if (client == null)
            throw new Exception("ImageAnnotatorClient is null!");
        return await client.DetectSafeSearchAsync(image);
    }

    public async Task<SafeSearchAnnotation?> PerformSafeSearch(RevoltClient revoltClient, RevoltFile file)
    {
        var extension = file.Filename.Split(".").Last();
        var url = $"{revoltClient.EndpointNodeInfo.Features.Autumn.Url}/{file.Tag}/{file.Id}/{file.Filename}";
        if (AnnotationCache.TryGetValue(url, out var cachedData))
            return cachedData;
        return await PerformSafeSearch(url);
    }

    private async Task<GoogleCredential?> ParseCredential(GoogleCloudKey? key)
    {
        if (key == null)
        {
            Log.Warn("Given cloud key is null, so the result is null btw");
            return null;
        }

        if (key.ProjectId.Length < 3)
        {
            Log.Warn("key.ProjectId.Length is < 3    Are you sure your config is correct?");
        }

        GoogleCredential? cred = null;
        using (CancellationTokenSource source = new CancellationTokenSource())
        {
            var jsonText = JsonSerializer.Serialize(key, Program.SerializerOptions);
            if (jsonText == null)
            {
                Log.Error("Failed to serialize Google Cloud Config.");
                Program.Quit(1);
                return null;
            }
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonText)))
            {
                try
                {
                    cred = await GoogleCredential.FromStreamAsync(memoryStream, source.Token);
                }
                catch (Exception ex)
                {
                    string errorMessage = "Failed to create GoogleCredential";
                    Log.Error(errorMessage);
                    Log.Error(ex);
#if DEBUG
                    throw;
#endif
                    Program.Quit(1);
                }
            }
        }
        if (cred == null)
        {
            string errorMessage = "Object \"cred\" is null. (Failed to create credentials)";
            Log.Error(errorMessage);
#if DEBUG
            throw new Exception(errorMessage);
#endif
            Program.Quit(1);
            return null;
        }
        return cred;
    }
}