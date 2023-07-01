using System.Net;
using System.Text.Json;
using RevSharp.Xenia.GoogleCloud.Perspective.Models;

namespace RevSharp.Xenia.Modules;

public partial class GoogleApiController
{
    internal string? GetPerspectiveAPIKey()
    {
        var v = Reflection.Config.GoogleCloud.PerspectiveAPIKey;
        return v;
    }

    public async Task<AnalyzeCommentResponse> AnalyzeComment(AnalyzeCommentRequest data)
    {
        var jsonText = JsonSerializer.Serialize(data, Client.PutSerializerOptionsL);
        var content = new StringContent(jsonText, null, "application/json");
        var apiKey = GetPerspectiveAPIKey();
        if (apiKey == null)
            throw new Exception("API Key not set");

        var url = $"https://commentanalyzer.googleapis.com/v1alpha1/comments:analyze?key={apiKey}";
        var client = new HttpClient();
        var response = await client.PostAsync(url, content);
        var stringContent = response.Content.ReadAsStringAsync().Result;
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception($"Failed to analyze comment\n{stringContent}");
        }

        var deser = JsonSerializer.Deserialize<AnalyzeCommentResponse>(stringContent, Client.PutSerializerOptionsL);
        if (deser == null)
            throw new Exception($"Failed to deserialize AnalyzeCommentResponse\n{deser}");

        return deser;
    }
}