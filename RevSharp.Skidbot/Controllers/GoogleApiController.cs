using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Vision.V1;
using Grpc.Auth;
using RevSharp.Skidbot.Models;
using RevSharp.Skidbot.Reflection;
using RevoltFile = RevSharp.Core.Models.File;
using RevoltClient = RevSharp.Core.Client;
using VisionImage = Google.Cloud.Vision.V1.Image;

namespace RevSharp.Skidbot.Modules;

[RevSharpModule]
public partial class GoogleApiController : BaseModule
{
    public GoogleCredential CredentialDefault { get; private set; }
    public override async Task Initialize(ReflectionInclude reflection)
    {
        await GetDefaultCredential();
        await GetVisionCredential();
        await GetStorageClient();
        await GetImageAnnotator();
    }

    public override bool WaitForInit => false;

    public async Task<GoogleCredential> GetDefaultCredential()
    {
        var cred = CredentialDefault ??
                await ParseCredential(Program.ConfigData.GoogleCloud.DefaultCred);

        if (cred == null)
        {
            Log.Error("CredentialDefault was parsed to null. Aborting Process");
            Program.Quit(1);
            return null;
        }

        CredentialDefault = cred;
        return CredentialDefault;
    }


    private readonly HttpClient _httpClient = new HttpClient();
    
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