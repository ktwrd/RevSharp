using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using RevSharp.Skidbot.Helpers;
using RevSharp.Skidbot.Models;
using StorageObject = Google.Apis.Storage.v1.Data.Object;

namespace RevSharp.Skidbot.Modules;

public partial class GoogleApiController
{
    private StorageClient? _storageClient;
    private GoogleCloudKey? _storageKey;
    private GoogleCredential? _storageCredential;

    public async Task<StorageClient?> GetStorageClient()
    {
        if (_storageClient != null)
            return _storageClient;

        var creds = await GetDefaultCredential();
        _storageCredential = creds;
        _storageKey = Program.ConfigData.GoogleCloud.DefaultCred;
        var client = await StorageClient.CreateAsync(creds);
        _storageClient = client;
        return _storageClient;
    }

    public bool BucketExists(string bucketName)
    {
        var buckets = _storageClient.ListBuckets(_storageKey.ProjectId);
        return buckets.All(v => v.Id != bucketName);
    }

    private HttpClient _storageHttpClient = new HttpClient();
    public async Task<StorageObject?> UploadToBucket(string url, string bucket)
    {
        var httpRes = await _storageHttpClient.GetAsync(url);
        var content = httpRes.Content.ReadAsByteArrayAsync().Result;
        var contentType = httpRes.Content.Headers.ContentType.ToString();
        var hash = SkidbotHelper.CreateSha256Hash(content);
        AddToCache(hash, url, contentType, content);
        if (!BucketExists(Program.ConfigData.ContentDetectionBucket))
        {
            await _storageClient.CreateBucketAsync(_storageKey.ProjectId, Program.ConfigData.ContentDetectionBucket);
        }

        using (var ms = new MemoryStream(content))
        {
            var obj = _storageClient.UploadObject(Program.ConfigData.ContentDetectionBucket, hash, contentType, ms);
            return obj;
        }
        
    }
}