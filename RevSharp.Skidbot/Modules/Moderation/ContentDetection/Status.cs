using RevSharp.Core.Models;
using RevSharp.Skidbot.Helpers;
using RevSharp.Skidbot.Models.ContentDetection;

namespace RevSharp.Skidbot.Modules;

public partial class ContentDetectionModule
{
    private async Task Command_Status(CommandInfo info, Message message)
    {
        var embed = new SendableEmbed()
        {
            Title = "Content Detection - Status"
        };
        
        var server = await message.FetchServer();
        var controller = Reflection.FetchModule<ContentDetectionServerConfigController>();
        var data = await controller.Get(server.Id) ??
                   new AnalysisServerConfig()
                   {
                       ServerId = server.Id
                   };

        embed.Description = string.Join(
            "\n", new string[]
            {
                "```", $"   Can Enable: {data.AllowAnalysis}", $"   Enabled   : {data.Enabled}",
                $"   Banned    : {data.IsBanned}",
                data is
                {
                    HasRequested: true,
                    AllowAnalysis: false,
                    IsBanned: false
                }
                    ? " Awaiting Manual Approval"
                    : "",
                "```"
            });
        await message.Reply(embed);
    }
}