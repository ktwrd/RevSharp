using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Google.Cloud.Storage.V1;
using Google.Cloud.Vision.V1;
using kate.shared.Helpers;
using RevSharp.Core.Models;
using RevSharp.Core.Models.WebSocket;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Models.ContentDetection;
using RevSharp.Xenia.Reflection;
using RevoltClient = RevSharp.Core.Client;

using RevoltFile = RevSharp.Core.Models.File;

namespace RevSharp.Xenia.Modules;

[RevSharpModule]
public partial class ContentDetectionModule : BaseModule
{
    public override bool HasHelpContent => FeatureFlags.EnableContentDetection;
    public override string? InternalName => "condetect";
    public override string? HelpCategory => "moderation";
    public override string? BaseCommandName => "condetect";

    public override string? HelpContent()
    {
        var p = Reflection.Config.Prefix;
        return string.Join("\n", new string[]
        {
            "```",
            " ------ server status --------",
            $"{p}condetect status        - Get content detection",
            "                          - status for this server",
            $"{p}condetect request       - Request content detection",
            "                          - to be enabled for this server",
            " ------- toggle server -------",
            $"{p}condetect disable       - Disable content detection",
            "                          - for this server",
            $"{p}condetect enable        - Enable content detection",
            "                          - for this server",
            "",
            " ---------- config -----------",
            $"see {p}cdconfig help",
            "",
            " ----------- help ------------",
            $"{p}condetect help          - Display this message",
            $"{p}condetect               - Display this message",
            "```"
        });
    }

    public override async Task CommandReceived(CommandInfo info, Message message)
    {
        // Ignore when ContentDetection is disabled
        if (!FeatureFlags.EnableContentDetection)
            return;
        if (info == null || info.Command != InternalName)
            return;

        var embed = new SendableEmbed()
        {
            Title = "Content Detection Module"
        };
        
        var action = info.Arguments.FirstOrDefault() ?? "help";
        var prefix = Reflection.Config.Prefix;
        if (action == "help")
        {
            embed.Title += " - Help";
            embed.Description = HelpContent();
            await message.Reply(embed);
        }
        else if (action == "thresholdget")
            await message.Reply($"Moved to the `{prefix}.cdadmin` command.");
        else if (action == "thresholdset")
            await message.Reply($"Moved to the `{prefix}.cdadmin` command.");
        else if (action == "setlogchannel")
            await message.Reply($"Moved to the `{prefix}.cdadmin` command.");
        else if (action == "status")
            await Command_Status(info, message);
        else if (action == "request")
            await Command_Request(info, message);
        else if (action == "admin")
            await Command_Admin(info, message);
        else
        {
            embed.Description = $"Action `{action}` not implemented";
            await message.Reply(embed);
        }
    }
    public override async Task MessageReceived(Message message)
    {
        // Ignore when ContentDetection is disabled
        if (!FeatureFlags.EnableContentDetection)
            return;

        var detectionController = Reflection.FetchModule<ContentDetectionController>();
        if (detectionController == null)
        {
            Log.Error($"ContentDetectionController not found!");
            return;
        }
        await detectionController.RunDetection(message);
    }

    public enum LogDetailReason
    {
        FlagThresholdMet,
        DeleteThresholdMet,
        Error
    }

}
