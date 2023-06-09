using RevSharp.Core.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Models.ContentDetection;

namespace RevSharp.Xenia.Modules;

public partial class ContentDetectionModule
{
    private async Task Command_Status(CommandInfo info, Message message)
    {
        await message.AddReaction(Client, "✅");
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
                "```",
                $"   Can Enable: {data.AllowAnalysis}",
                $"   Enabled   : {data.Enabled}",
                $"   Banned    : {data.IsBanned}",
                $"   Text      : {data.AllowTextDetection}",
                $"   Media Text:{data.AllowMediaTextDetection}",
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