using System.Diagnostics;
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
        var client = await StorageClient.CreateAsync(_storageCredential);
        _storageClient = client;
        return _storageClient;
    }

    public bool BucketExists(string bucketName)
    {
        var buckets = _storageClient.ListBuckets(_storageKey.ProjectId);
        return buckets.All(v => v.Id != bucketName);
    }

    private HttpClient _storageHttpClient = new HttpClient();
    /// <summary>
    /// Fetch the URL and the contents of it. Calculate a SHA256 hash for that content.
    ///
    /// The filename is the SHA256 hash.
    /// 
    /// If the bucket contains the file, then it just gets that
    ///
    /// If the bucket doesn't contain the file, we upload it.
    /// </summary>
    /// <param name="url">Url to download from</param>
    /// <param name="bucket">Bucket to upload/fetch from</param>
    /// <param name="validContentTypes">Allowed content types from the HTTP Response</param>
    /// <returns>Google Cloud Storage Object. null when failure.</returns>
    public async Task<StorageObject?> UploadToBucket(string url, string bucket, string[]? validContentTypes = null)
    {
        var httpRes = await _storageHttpClient.GetAsync(url);
        var content = httpRes.Content.ReadAsByteArrayAsync().Result;
        var contentType = httpRes.Content.Headers.ContentType.ToString();
        var contentTypeSingle = httpRes.Content.Headers.ContentType.MediaType;
        if (validContentTypes != null && !validContentTypes.Contains(contentTypeSingle))
            return null;
        
        var hash = GoogleApiController.ComputeSha256Hash(content);
        var testObjects = _storageClient.ListObjects(bucket, hash);
        if (testObjects.Count() > 0)
        {
            return await _storageClient.GetObjectAsync(bucket, hash);
        }
        
        AddToCache(hash, url, contentType);

        using (var ms = new MemoryStream(content))
        {
            var obj = _storageClient.UploadObject(bucket, hash, contentType, ms);
            return obj;
        }
    }
}