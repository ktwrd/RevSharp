using NetVips;
using RevSharp.Skidbot.Reflection;
using NVIPS = NetVips.NetVips;

namespace RevSharp.Skidbot.ImgWiz.Controllers;

[RevSharpModule]
public class ImageWizardController : BaseModule
{
    public override async Task Initialize(ReflectionInclude reflectionInclude)
    {
        if (ModuleInitializer.VipsInitialized)
        {
            Log.WriteLine($"Using libvips {NVIPS.Version(0)}.{NVIPS.Version(1)}.{NVIPS.Version(2)}");
        }
        else
        {
            Log.Error($"Failed to init libvips\n{ModuleInitializer.Exception.Message}");
        }
    }
}