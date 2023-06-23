using System.Diagnostics;
using System.Reactive.Linq;
using RevSharp.Core.Models;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Controllers;

[RevSharpModule]
public class PresenceController : BaseModule
{
    public override async Task Initialize(ReflectionInclude reflection)
    {
        Logic();
    }

    private async Task Logic()
    {
        bool isNull = true;
        while (isNull)
        {
            isNull = Client == null || Client.CurrentUser == null;
            await Task.Delay(500);
        }
        await SetPresence();
        Observable.Interval(TimeSpan.FromSeconds(360))
            .Subscribe(_ => SetPresence().Wait());
    }
    
    public async Task SetPresence()
    {
        try
        {
            var controller = Reflection.FetchModule<StatisticController>();
            int members = controller.TotalMemberCount;
            int servers = controller.ServerCount;
            await Client.CurrentUser.UpdatePresence(
                $"{Program.ConfigData.Prefix}help | {members} users, {servers} servers", UserPresence.Online);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to set presence\n{ex}");
        }
    }
}