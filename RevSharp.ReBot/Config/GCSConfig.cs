using RevSharp.ReBot.Models;

namespace RevSharp.ReBot;

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