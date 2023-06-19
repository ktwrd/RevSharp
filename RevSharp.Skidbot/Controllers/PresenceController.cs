using System.Diagnostics;
using System.Reactive.Linq;
using RevSharp.Core.Models;
using RevSharp.Skidbot.Reflection;

namespace RevSharp.Skidbot.Controllers;

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

    private int MemberCount = 0;
    private int ServerCount = 0;
    private async Task<int> GetUserCount()
    {
        int count = 0;
        var s = await Client.GetAllServers();
        foreach (var i in s)
        {
            var members = await i.FetchMembers(false);
            if (members != null)
                count += members.Count;
        }

        ServerCount = s.Count;
        MemberCount = count;

        return count;
    }
    private async Task SetPresence()
    {

        try
        {
            var count = await GetUserCount();
            var res = await Client.CurrentUser.UpdatePresence(
                $"{Program.ConfigData.Prefix}help | {count} users, {ServerCount} servers", UserPresence.Online);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to set presence\n{ex}");
        }
    }
}