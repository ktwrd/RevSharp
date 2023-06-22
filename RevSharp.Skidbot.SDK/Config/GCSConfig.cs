using RevSharp.Skidbot.Models;

namespace RevSharp.Skidbot;

public class GCSConfig
{
    public GoogleCloudKey DefaultCred { get; set; }
    public GoogleCloudKey? VisionAPI { get; set; }

    public GCSConfig()
    {
        DefaultCred = new GoogleCloudKey();

        VisionAPI = null;
    }
}