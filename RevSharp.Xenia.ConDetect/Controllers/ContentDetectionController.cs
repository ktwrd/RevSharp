using System.Text;
using System.Text.Json;
using kate.shared.Helpers;
using RevSharp.Core.Models;
using RevSharp.Xenia.GoogleCloud.Perspective.Models;
using RevSharp.Xenia.Models.ContentDetection;
using RevSharp.Xenia.Reflection;
using RevoltClient = RevSharp.Core.Client;

namespace RevSharp.Xenia.Modules;

[RevSharpModule]
public class ContentDetectionController : BaseModule
{
    public async Task AnalyzeMedia(AnalysisServerConfig config,Server server, Message message)
    {
        try
        {
            var analysis = await AnalyzeMessage(message);
            var deleteMatch = config.GetMessageThresholdMatch(analysis, config.DeleteThreshold);
            var flagMatch = config.GetMessageThresholdMatch(analysis, config.FlagThreshold);
            if (deleteMatch.Majority != null)
            {
                await WriteLogThreshold(
                    config,
                    ContentDetectionModule.LogDetailReason.DeleteThresholdMet,
                    deleteMatch,
                    message,
                    "",
                    analysis);

                try
                {
                    if (!await message.Delete())
                    {
                        var channel = await Client.GetChannel(config.LogChannelId) as TextChannel;
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
                    var channel = await Client.GetChannel(config.LogChannelId) as TextChannel;
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
                    await ReportError(
                        ex, message,
                        $"Failed to delete message {message.Id} in channel {message.ChannelId} in server {server.Id}");
                }
            }
            else if (flagMatch.Majority != null)
            {
                await WriteLogThreshold(
                    config,
                    ContentDetectionModule.LogDetailReason.FlagThresholdMet,
                    flagMatch,
                    message,
                    "",
                    analysis);
            }
        }
        catch (Exception ex)
        {
            await ReportError(ex, message, "Failed to call RunDetection");
            await WriteLine(config, $"Failed to call RunDetection\n```\n{ex}\n```");
        }
    }

    public async Task AnalyzeText(AnalysisServerConfig config, Server server, Message message)
    {
        if (!config.AllowTextDetection)
            return;
        if (string.IsNullOrEmpty(message.Content))
            return;

        var googleController = Reflection.FetchModule<GoogleApiController>();
        if (googleController == null)
        {
            Log.Warn("GoogleApiController is null ;w;");
            return;
        }
        var requestData = new AnalyzeCommentRequest()
            .WithText(message.Content)
            .AddAllAttrs();
        requestData.CommunityId = server.Id;
        var result = await googleController.AnalyzeComment(requestData);

        Dictionary<string, AnalyzeCommentScore> GenerateMatchDictionary(Dictionary<string, float> thresholds)
        {
            var dict = new Dictionary<string, AnalyzeCommentScore>();
            foreach (var pair in result.AttributeScores)
            {
                if (!thresholds.TryGetValue(pair.Key.ToString(), out var threshVal))
                    continue;
                if (threshVal < 0f)
                    continue;
                if (pair.Value.Summary.Value >= threshVal)
                    dict.Add(pair.Key.ToString(), pair.Value);
            }

            return dict;
        }
        var deleteMatches = GenerateMatchDictionary(config.TextDeleteThreshold);
        if (deleteMatches.Count > 0)
            await message.Delete();
        var flagMatches = GenerateMatchDictionary(config.TextFlagThreshold);
        
        if (flagMatches.Count < 1 && deleteMatches.Count < 1)
            return;
        
        var descriptionLines = new List<string>()
        {
            $"Message from <@{message.AuthorId}> in <#{message.ChannelId}>.",
            "",
            "Content;",
            "```",
            message.Content.Replace("`", "\\`"),
            "```",
        };
        if (deleteMatches.Count > 0)
        {
            descriptionLines.AddRange(new string[]
            {
                "Delete Matches;",
                string.Join("\n", deleteMatches.Select(v => $"> `{v.Key} = {Math.Round(v.Value.Summary.Value * 100, 2)}`")),
            });
        }

        if (flagMatches.Count > 0)
        {
            descriptionLines.AddRange(new string[]
            {
                "Flag Matches;",
                string.Join("\n", flagMatches.Select(v => $"> `{v.Key} = {Math.Round(v.Value.Summary.Value * 100, 2)}%`")),
            });
        }
        descriptionLines.AddRange(new string[]
        {
            "Action;",
            "> `" + (deleteMatches.Count > 0 ? "`Deleted`" : "`Flagged`") + "`"
        });
        var embed = new SendableEmbed()
        {
            Title = "Content Detection - Hate Speech Detected",
            Description = string.Join("\n",descriptionLines)
        };
        
        var channel = await Client.GetChannel(config.LogChannelId) as TextChannel;
        var file = await Client.UploadFile(
            new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result, RevoltClient.SerializerOptions))), "analysis.json",
            "attachments");
        
        await channel.SendMessage(new DataMessageSend()
        {
            Attachments = new string[]{ file },
            Embeds = new SendableEmbed[] {embed}
        });
    }
    public async Task RunDetection(Message message)
    {
        var server = await message.FetchServer();
        if (server == null)
            return;
        var controller = Reflection.FetchModule<ContentDetectionServerConfigController>();
        if (controller == null)
            return;

        var serverConfig = await controller.Fetch(server.Id);
        if (serverConfig == null)
            serverConfig = new AnalysisServerConfig()
            {
                ServerId = server.Id
            };
        await controller.Set(serverConfig);
        if (serverConfig == null || serverConfig.IsBanned || !serverConfig.Enabled)
            return;
        if (message.AuthorId == Client.CurrentUserId)
            return;
        if (serverConfig.IgnoredAuthorIds.Contains(message.AuthorId.ToUpper()))
            return;
        if (serverConfig.IgnoredChannelIds.Contains(message.ChannelId.ToUpper()))
            return;

        try
        {
            await AnalyzeText(serverConfig, server, message);
            await AnalyzeMedia(serverConfig, server, message);
        }
        catch (Exception ex)
        {
            await ReportError(ex, message, "Failed to run content detection");
        }
    }
    
    public async Task<AnalysisResult?> AnalyzeMessage(Message message)
    {
        var startTs = GeneralHelper.GetMicroseconds() / 1000;
        var googleController = Reflection.FetchModule<GoogleApiController>();

        var result = new AnalysisResult();

        async Task ProcessUrl(string url, string tag)
        {
            var (data, hash) = await googleController.PerformSafeSearch(url);
            if (data != null)
            {
                result.AddAnnotation(data, tag);
                result.HashList.Add((tag, hash));
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

        
        
        if ((message.Content?.Contains("r.condebug") ??
             false) && Reflection.Config.OwnerUserIds.Contains(message.AuthorId))
            await message.Reply(
                "```\n" + string.Join("\n", result.Annotations.Select(
                    (v) =>
                    {
                        var pairs = JsonSerializer.Deserialize<Dictionary<string, string>>(
                                JsonSerializer.Serialize(v.Item1, Client.SerializerOptionsL), Client.SerializerOptionsL)
                            .Select(a => $"  - {a.Key,-9}={a.Value}");
                        return $"- {v.Item2}\n" + string.Join("\n", pairs);
                    })) + "\n```");

        Log.Debug($"Took {(GeneralHelper.GetMicroseconds() / 1000) - startTs}ms");
        return result;
    }
    public async Task WriteLine(AnalysisServerConfig config, string content)
    {
        var channel = await Client.GetChannel(config.LogChannelId) as TextChannel;
        if (channel == null)
            return;
        await channel.SendMessage(new DataMessageSend()
        {
            Content = content.Substring(0, Math.Min(2000, content.Length))
        });
    }
    public async Task WriteLogThreshold(
        AnalysisServerConfig serverConfig,
        ContentDetectionModule.LogDetailReason reason,
        ContentAnalysisMessageMatch match,
        Message message,
        string otherContent = "",
        AnalysisResult? analysis = null)
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
                ContentDetectionModule.LogDetailReason.DeleteThresholdMet => "Deletion threshold met",
                ContentDetectionModule.LogDetailReason.FlagThresholdMet => "Flag threshold met",
                _ => reason.ToString()
            };
            var server = await message.FetchServer();

            var user = await Client.GetUser(message.AuthorId, false);
            if (reason != ContentDetectionModule.LogDetailReason.Error)
            {
                embed.Description = string.Join(
                    "\n", new string[]
                    {
                        "Info",
                        $"`Channel:    {message.ChannelId}`",
                        $"`Author:     {user.Username}#{user.Discriminator} ({message.AuthorId})`",
                        $"`Message Id: {message.Id}`",
                        "",
                        "Detections",
                        string.Join("\n", match.MajorityPairs.Select(v => $"`{v.Key} {v.Value}`")),
                        $"[Jump to Message](https://app.revolt.chat/server/{server.Id}/channel/{message.ChannelId}/{message.Id})",
                        "Hashes",
                        string.Join("\n", analysis?.HashList.Select(v => $"`{v.Item1} {v.Item2}`") ?? Array.Empty<string>()),
                        "Matches",
                        "```",
                        string.Join("\n", analysis?.Annotations.Select(
                            (v) =>
                            {
                                var pairs = JsonSerializer.Deserialize<Dictionary<string, string>>(
                                        JsonSerializer.Serialize(v.Item1, Client.SerializerOptionsL), Client.SerializerOptionsL)
                                    .Select(a => $"  - {a.Key,-9}={a.Value}");
                                return $"- {v.Item2}\n" + string.Join("\n", pairs);
                            })),
                        "```"
                    });
            }
            else
            {
                embed.Description = $"```\n{otherContent}\n```";
            }
            embed.Colour = reason switch
            {
                ContentDetectionModule.LogDetailReason.DeleteThresholdMet => "red",
                ContentDetectionModule.LogDetailReason.FlagThresholdMet => "orange",
                ContentDetectionModule.LogDetailReason.Error => "white"
            };

            var channel = await Client.GetChannel(serverConfig.LogChannelId) as TextChannel;
            var file = await Client.UploadFile(
                new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(match, RevoltClient.SerializerOptions))), "match.json",
                "attachments");
            await channel.SendMessage(new DataMessageSend()
            {
                Embeds = new []{embed},
                Attachments = file != null ? new []{file} : Array.Empty<string>()
            });
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to send log threshold message\n{ex}");
            await ReportError(
                ex, message,
                $"Failed to send log threshold message");
        }
    }
}