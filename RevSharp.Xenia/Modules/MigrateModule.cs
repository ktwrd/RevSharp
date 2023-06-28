using RevSharp.Core.Models;
using RevSharp.Xenia.Controllers;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Modules;

[RevSharpModule]
public class MigrateModule : CommandModule
{
    public override async Task CommandReceived(CommandInfo info, Message message)
    {
        string action = "";
        if (info.Arguments.Count > 0)
            action = info.Arguments[0].ToLower();

        switch (action)
        {
            case "level":
                await Reflection.FetchModule<LevelSystemUserController>().Migrate();
                await message.Reply("Done!");
                break;
            default:
                await message.Reply($"Migrate task `{action,-1}` is not implemented");
                break;
        }
    }

    public override string? HelpContent()
    {
        return "";
    }

    public override bool HasHelpContent => true;
    public override string? HelpCategory => "owner";
    public override string? BaseCommandName => "migrate";
    public override bool WaitForInit => false;
    public override bool OwnerOnly => true;
}