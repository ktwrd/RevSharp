using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Google.Cloud.Vision.V1;
using kate.shared.Helpers;
using RevSharp.Core.Models;
using RevSharp.Core.Models.WebSocket;
using RevSharp.ReBot.Helpers;
using RevSharp.ReBot.Models.ContentDetection;
using RevSharp.ReBot.Reflection;

using RevoltFile = RevSharp.Core.Models.File;

namespace RevSharp.ReBot.Modules;

[RevSharpModule]
public partial class ContentDetectionModule : BaseModule
{
    public override bool HasHelpContent => FeatureFlags.EnableContentDetection;
    public override string? InternalName => "condetect";
    public override string? HelpCategory => "moderation";

    public override string? HelpContent()
    {
        var p = Program.ConfigData.Prefix;
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
            $"{p}condetect setlogchannel  - Set current channel to",
            "                           - logging channel",
            $"{p}condetect thresholdset <type> <threshold> <action>",
            "                           - Set detection threshold",
            $"{p}condetect thresholdget   - Get thresholds",
            "",
            " ----------- help ------------",
            $"{p}condetect help          - Display this message",
            $"{p}condetect               - Display this message",
            "```"
        });
    }

    public override async Task MessageReceived(Message message)
    {
        // Ignore when ContentDetection is disabled
        if (!FeatureFlags.EnableContentDetection)
            return;
        ContentDetectionTask(message);

        var info = CommandHelper.FetchInfo(message);
        if (info == null || info.Command != InternalName)
            return;

        var embed = new SendableEmbed()
        {
            Title = "Content Detection Module"
        };
        
        var action = info.Arguments.FirstOrDefault() ?? "help";
        if (action == "help")
        {
            embed.Title += " - Help";
            embed.Description = HelpContent();
            await message.Reply(embed);
        }
        else if (action == "thresholdget")
            await Command_ThresholdGet(info, message);
        else if (action == "thresholdset")
            await Command_ThresholdSet(info, message);
        else if (action == "setlogchannel")
            await Command_SetLogChannel(info, message);
        else if (action == "status")
            await Command_Status(info, message);
        else if (action == "request")
            await Command_Request(info, message);
        else
        {
            embed.Description = $"Action `{action}` not implemented";
            await message.Reply(embed);
        }
    }

    public async Task ContentDetectionTask(Message message)
    {
        var server = await message.FetchServer();
        if (server == null)
            return;
        #if DEBUG
        if (server?.Id != "01G5XEAEJSEXKXCNTF8E5B004A")
            return;
        #endif
        var controller = Reflection.FetchModule<ContentDetectionServerConfigController>();
        if (controller == null)
            return;

        var serverConfig = await controller.Get(server.Id);
        if (serverConfig == null || serverConfig.Enabled == false)
            return;
        if (message.AuthorId == Client.CurrentUserId)
            return;

        var analysis = await AnalyzeMessage(message);
        var deleteMatch = serverConfig.GetMessageThresholdMatch(analysis, serverConfig.DeleteThreshold);
        var flagMatch = serverConfig.GetMessageThresholdMatch(analysis, serverConfig.FlagThreshold);
        if (deleteMatch.Majority != null)
        {
            message.Delete();
            WriteLogThreshold(
                serverConfig,
                LogDetailReason.DeleteThresholdMet,
                deleteMatch,
                message);
        }
        else if (flagMatch.Majority != null)
        {
            WriteLogThreshold(
                serverConfig,
                LogDetailReason.FlagThresholdMet,
                flagMatch,
                message);
        }
    }

    public async Task WriteLogThreshold(
        AnalysisServerConfig serverConfig,
        LogDetailReason reason,
        ContentAnalysisMessageMatch match,
        Message message)
    {
        var embed = new SendableEmbed()
        {
            Title = "Threshold Reached!"
        };
        embed.Title += " ";
        embed.Title += reason switch
        {
            LogDetailReason.DeleteThresholdMet => "Deletion threshold met",
            LogDetailReason.FlagThresholdMet => "Flag threshold met"
        };

        var user = await Client.GetUser(message.AuthorId);
        embed.Description = string.Join(
            "\n", new string[]
            {
                "Info",
                "```",
                $"Channel:    {message.ChannelId}",
                $"Author:     {user.Username} ({message.AuthorId})",
                $"Message Id: {message.Id}",
                "```",
                "",
                "Detections",
                "```",
                string.Join("\n", match.MajorityItems),
                "```",
                "",
                "Threshold Data",
                "```json",
            }) + JsonSerializer.Serialize(match, Program.SerializerOptions) + "\n```";
        embed.Colour = reason switch
        {
            LogDetailReason.DeleteThresholdMet => "red",
            LogDetailReason.FlagThresholdMet => "orange"
        };

        var channel = await Client.GetChannel(serverConfig.LogChannelId) as TextChannel;
        await channel.SendMessage(embed);
    }

    public enum LogDetailReason
    {
        FlagThresholdMet,
        DeleteThresholdMet
    }

    public async Task<AnalysisResult?> AnalyzeMessage(Message message)
    {
        var startTs = GeneralHelper.GetMicroseconds() / 1000;
        var googleController = Reflection.FetchModule<GoogleApiController>();

        var result = new AnalysisResult();

        async Task ProcessUrl(string url)
        {
            var data = await googleController.PerformSafeSearch(url);
            if (data != null)
            {
                result.AddAnnotation(data);
            }
        }

        string ParseRevoltFile(RevoltFile file)
        {
            return $"{Client.EndpointNodeInfo.Features.Autumn.Url}/{file.Tag}/{file.Id}/{file.Filename}";
        }
        
        var taskList = new List<Task>();
        if (message.Attachments != null)
        {
            foreach (var file in message.Attachments)
            {
                taskList.Add(new Task(delegate
                {
                    ProcessUrl(ParseRevoltFile(file)).Wait();
                }));
            }
        }

        if (message.Embeds != null)
        {
            foreach (var imageItem in message.Embeds.OfType<ImageEmbed>())
            {
                taskList.Add(new Task(
                    delegate
                    {
                        ProcessUrl(imageItem.Url).Wait();
                    }));
            }

            foreach (var metadataItem in message.Embeds.OfType<MetadataEmbed>())
            {
                taskList.Add(new Task(
                    delegate
                    {
                        if (metadataItem.OriginalUrl != null)
                            ProcessUrl(metadataItem.OriginalUrl).Wait();
                        if (metadataItem.Url != null)
                            ProcessUrl(metadataItem.Url).Wait();
                        if (metadataItem.IconUrl != null)
                            ProcessUrl(metadataItem.IconUrl).Wait();
                        if (metadataItem.Image != null)
                            if (metadataItem.Image.Url != null)
                                ProcessUrl(metadataItem.Image.Url).Wait();
                    }));
            }

            foreach (var textEmbed in message.Embeds.OfType<TextEmbed>())
            {
                taskList.Add(new Task(
                    delegate
                    {
                        if (textEmbed.Url != null)
                            ProcessUrl(textEmbed.Url).Wait();
                        if (textEmbed.IconUrl != null)
                            ProcessUrl(textEmbed.IconUrl).Wait();
                        if (textEmbed.Media != null)
                            ProcessUrl(ParseRevoltFile(textEmbed.Media)).Wait();
                    }));
            }
        }

        foreach (var i in taskList)
            i.Start();
        await Task.WhenAll(taskList);
        
        if (result.Total < 1)
            return result;
        
#if DEBUG
        try
        {
            var data = JsonSerializer.Serialize(result, Program.SerializerOptions);
            await message.Reply($"```json\n{data}\n```");
        }
        catch (Exception ex)
        {
            Log.Error(ex);
            Debugger.Break();
        }
#endif

        Log.Debug($"Took {(GeneralHelper.GetMicroseconds() / 1000) - startTs}ms");

        return result;
    }
}
