using System.Text.Json;
using RevSharp.Core.Models;
using RevSharp.Xenia.GoogleCloud.Perspective.Models;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Modules;

[RevSharpModule]
public class ConDetectDebugModule : CommandModule
{
    public override async Task CommandReceived(CommandInfo info, Message message)
    {
        string action = "";
        if (info.Arguments.Count > 0)
            action = info.Arguments[0];
        switch (action)
        {
            case "perspective":
                await Command_Perspective(info, message);
                break;
            case "help":
                await message.Reply(GenerateHelp());
                break;
            default:
                await message.Reply(GenerateHelp().WithContent($"Unknown action `{action,-1}`"));
                break;
        }
    }

    public async Task Command_Perspective(CommandInfo info, Message message)
    {
        try
        {
            string content = CommandHelper.FetchContent(info, 1);
            var controller = Reflection.FetchModule<GoogleApiController>();
            var response = await controller.AnalyzeComment(new AnalyzeCommentRequest(content).AddAllAttrs());
            var ser = JsonSerializer.Serialize(response, Client.SerializerOptionsLI);
            await message.Reply($"```json\n{ser}\n```");
        }
        catch (Exception ex)
        {
            await message.Reply($"Failed to run perspective\n```\n{ex}\n```");
        }
    }

    public DataMessageSend GenerateHelp()
    {
        var embed = new SendableEmbed()
        {
            Title = "Content Detection Debug",
            Description = HelpContent()
        };
        return new DataMessageSend().AddEmbed(embed);
    }
    public override string? HelpContent()
    {
        return "";
    }
    public override bool OwnerOnly => true;
    public override string? BaseCommandName => "condebug";
    public override bool HasHelpContent => true;
    public override string HelpCategory => "admin";
}