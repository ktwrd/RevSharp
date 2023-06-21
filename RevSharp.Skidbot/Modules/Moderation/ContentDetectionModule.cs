using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Google.Cloud.Storage.V1;
using Google.Cloud.Vision.V1;
using kate.shared.Helpers;
using RevSharp.Core.Models;
using RevSharp.Core.Models.WebSocket;
using RevSharp.Skidbot.Helpers;
using RevSharp.Skidbot.Models.ContentDetection;
using RevSharp.Skidbot.Reflection;

using RevoltFile = RevSharp.Core.Models.File;

namespace RevSharp.Skidbot.Modules;

[RevSharpModule]
public partial class ContentDetectionModule : BaseModule
{
    public override bool HasHelpContent => FeatureFlags.EnableContentDetection;
    public override string? InternalName => "condetect";
    public override string? HelpCategory => "moderation";
    public override string? BaseCommandName => "condetect";

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
        ContentDetectionTask(message);
    }

    public async Task ContentDetectionTask(Message message)
    {
        var server = await message.FetchServer();
        if (server == null)
            return;
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
            WriteLogThreshold(
                serverConfig,
                LogDetailReason.DeleteThresholdMet,
                deleteMatch,
                message);

            try
            {
                if (!await message.Delete())
                {
                    var channel = await Client.GetChannel(serverConfig.LogChannelId) as TextChannel;
                    await channel.SendMessage(new SendableEmbed()
                    {
                        Title = "Failed to delete message!",
                        Description = string.Join("\n", new string[]
                        {
                            $"Failed to delete message `{message.Id}` in <#{message.ChannelId}>."
                        })
                    });
                }
            }
            catch (Exception ex)
            {
                var channel = await Client.GetChannel(serverConfig.LogChannelId) as TextChannel;
                await channel.SendMessage(new SendableEmbed()
                {
                    Title = "Failed to delete message!",
                    Description = string.Join("\n", new string[]
                    {
                        $"Failed to delete message `{message.Id}` in <#{message.ChannelId}>.",
                        "```",
                        ex.Message,
                        "```"
                    })
                });
            }
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
        Message message,
        string otherContent = "")
    {
        try
        {
            var embed = new SendableEmbed()
            {
                Title = "Threshold Reached!"
            };
            embed.Title += " ";
            embed.Title += reason switch
            {
                LogDetailReason.DeleteThresholdMet => "Deletion threshold met",
                LogDetailReason.FlagThresholdMet => "Flag threshold met",
                _ => reason.ToString()
            };

            var user = await Client.GetUser(message.AuthorId, false);
            if (reason != LogDetailReason.Error)
            {
                embed.Description = string.Join(
                    "\n", new string[]
                    {
                        "Info",
                        "```",
                        $"Channel:    {message.ChannelId}",
                        $"Author:     {user.Username}#{user.Discriminator} ({message.AuthorId})",
                        $"Message Id: {message.Id}",
                        "```",
                        "",
                        "Detections",
                        "```",
                        string.Join("\n", match.MajorityItems),
                        "```"
                    });
            }
            else
            {
                embed.Description = $"```\n{otherContent}\n```";
            }
            embed.Colour = reason switch
            {
                LogDetailReason.DeleteThresholdMet => "red",
                LogDetailReason.FlagThresholdMet => "orange",
                LogDetailReason.Error => "white"
            };

            var channel = await Client.GetChannel(serverConfig.LogChannelId) as TextChannel;
            var file = await Client.UploadFile(
                new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(match, Program.SerializerOptions))), "match.json",
                "attachments", "application/json");
            await channel.SendMessage(new DataMessageSend()
            {
                Embeds = new []{embed},
                Attachments = file != null ? new []{file} : Array.Empty<string>()
            });
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to send log threshold message\n{ex}");
        }
    }

    public enum LogDetailReason
    {
        FlagThresholdMet,
        DeleteThresholdMet,
        Error
    }

    public async Task<AnalysisResult?> AnalyzeMessage(Message message)
    {
        var startTs = GeneralHelper.GetMicroseconds() / 1000;
        var googleController = Reflection.FetchModule<GoogleApiController>();

        var result = new AnalysisResult();

        async Task ProcessUrl(string url, string tag)
        {
            var data = await googleController.PerformSafeSearch(url);
            if (data != null)
            {
                result.AddAnnotation(data, tag);
            }
        }
        
        var taskList = new List<Task>();
        if (message.Attachments != null)
        {
            for (int i = 0; i < message.Attachments.Length; i++)
            {
                var ii = int.Parse(i.ToString());
                taskList.Add(new Task(delegate
                {
                    string u = message.Attachments[ii].GetURL(Client);
                    ProcessUrl(u, $"message.Attachments[{ii}]").Wait();
                }));
            }
        }

        if (message.Embeds != null)
        {
            var messageImageEmbeds = message.Embeds.OfType<ImageEmbed>().ToArray();
            for (int i = 0; i < messageImageEmbeds.Length; i++)
            {
                var ii = int.Parse(i.ToString());
                taskList.Add(new Task(
                    delegate
                    {
                        ProcessUrl(messageImageEmbeds[ii].Url, $"message.Embeds[{ii}].Url").Wait();
                    }));
            }

            var messageMetaEmbeds = message.Embeds.OfType<MetadataEmbed>().ToArray();
            for (int i = 0; i < messageMetaEmbeds.Length; i++)
            {
                var itm = messageMetaEmbeds[i];
                var ii = int.Parse(i.ToString());
                taskList.Add(new Task(
                    delegate
                    {
                        if (itm.OriginalUrl != null)
                            ProcessUrl(itm.OriginalUrl, $"message.Embeds[{ii}].OriginalUrl").Wait();
                        if (itm.Url != null)
                            ProcessUrl(itm.Url, $"message.Embeds[{ii}].Url").Wait();
                        if (itm.IconUrl != null)
                            ProcessUrl(itm.IconUrl, $"message.Embeds[{ii}].IconUrl").Wait();
                        if (itm.Image != null)
                            if (itm.Image.Url != null)
                                ProcessUrl(itm.Image.Url, $"message.Embeds[{ii}].Image.Url").Wait();
                    }));
            }

            var messageTextEmbeds = message.Embeds.OfType<TextEmbed>().ToArray();
            for (int i = 0; i < messageTextEmbeds.Length; i++)
            {
                var itm = messageTextEmbeds[i];
                var ii = int.Parse(i.ToString());
                taskList.Add(new Task(
                    delegate
                    {
                        if (itm.Url != null)
                            ProcessUrl(itm.Url, $"message.Embeds[{ii}].Url").Wait();
                        if (itm.IconUrl != null)
                            ProcessUrl(itm.IconUrl, $"message.Embeds[{ii}].IconUrl").Wait();
                        if (itm.Media != null)
                            ProcessUrl(itm.Media.GetURL(Client), $"message.Embeds[{ii}].Media").Wait();
                    }));
            }
        }

        foreach (var i in taskList)
            i.Start();
        await Task.WhenAll(taskList);
        
        if (result.Total < 1)
            return result;

        Log.Debug($"Took {(GeneralHelper.GetMicroseconds() / 1000) - startTs}ms");

        return result;
    }
}
