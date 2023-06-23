using System.Text;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using RevSharp.Xenia.Models;

namespace RevSharp.Xenia.Helpers;

public static class GoogleHelper
{
    public static async Task<GoogleCredential?> ParseCredential(GoogleCloudKey? key)
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