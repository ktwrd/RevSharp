using System.Text.Json;
using Google.Cloud.Vision.V1;
using kate.shared.Helpers;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RevSharp.Xenia.Modules;

public partial class GoogleApiController
{
    /// <summary>
    /// Key: SHA256 Hash of Content    OR    URL
    /// Value: Cached Annotations
    /// </summary>
    protected Dictionary<string, SafeSearchAnnotation> _annotationCache = new Dictionary<string, SafeSearchAnnotation>();
    
    /// <summary>
    /// Key: SHA256 of Byte Array
    /// Value: MIME Type
    /// </summary>
    protected Dictionary<string, string> _safeSearchHashType = new Dictionary<string, string>();

    /// <summary>
    /// Key: Url
    /// Value: SHA256 Hash of content
    /// </summary>
    protected Dictionary<string, string> _urlContentHashCache = new Dictionary<string, string>();
    
    #region File Locations
    private string _annotationCacheLocation =>
        Path.Join(
            FeatureFlags.ImageAnnotatorCacheDirectory,
            "ac.json");

    private string _safeSearchHashTypeLocation =>
        Path.Join(
            FeatureFlags.ImageAnnotatorCacheDirectory,
            "ssht.json");

    private string _urlContentHashCacheLocation =>
        Path.Join(
            FeatureFlags.ImageAnnotatorCacheDirectory,
            "uch.json");
    #endregion
    
    private async Task SaveCache()
    {
        var startTs = GeneralHelper.GetMicroseconds() / 1000;
        if (!Directory.Exists(FeatureFlags.ImageAnnotatorCacheDirectory))
            Directory.CreateDirectory(FeatureFlags.ImageAnnotatorCacheDirectory);
        
        var jsonOptions = new JsonSerializerOptions()
        {
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            IncludeFields = true
        };

        Task InnerSaveLogic(string location, object data) =>
            File.WriteAllTextAsync(
                path: location,
                contents: JsonSerializer.Serialize(data, jsonOptions));

        var taskList = new List<Task>()
        {
            InnerSaveLogic(_annotationCacheLocation, _annotationCache),
            InnerSaveLogic(_safeSearchHashTypeLocation, _safeSearchHashType),
            InnerSaveLogic(_urlContentHashCacheLocation, _urlContentHashCache)
        };

        await Task.WhenAll(taskList);

        Log.Debug($"Took {(GeneralHelper.GetMicroseconds() / 1000) - startTs}ms");
    }

    private Task ReadCache()
    {
        var jsonOptions = new JsonSerializerOptions()
        {
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            IncludeFields = true
        };

        if (!Directory.Exists(FeatureFlags.ImageAnnotatorCacheDirectory))
            Directory.CreateDirectory(FeatureFlags.ImageAnnotatorCacheDirectory);
        
        if (File.Exists(_annotationCacheLocation))
        {
            _annotationCache =
                JsonSerializer.Deserialize<Dictionary<string, SafeSearchAnnotation>>(
                    File.ReadAllText(_annotationCacheLocation), jsonOptions);
        }

        if (File.Exists(_safeSearchHashTypeLocation))
        {
            _safeSearchHashType = JsonSerializer.Deserialize<Dictionary<string, string>>(
                File.ReadAllText(_safeSearchHashTypeLocation), jsonOptions);
        }

        if (File.Exists(_urlContentHashCacheLocation))
        {
            _urlContentHashCache = JsonSerializer.Deserialize<Dictionary<string, string>>(
                File.ReadAllText(_urlContentHashCacheLocation), jsonOptions);
        }

        _hasCacheRead = true;
        return Task.CompletedTask;
    }
}