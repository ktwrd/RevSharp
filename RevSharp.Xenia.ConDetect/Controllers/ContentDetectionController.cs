﻿using System.Text;
using System.Text.Json;
using kate.shared.Helpers;
using RevSharp.Core.Models;
using RevSharp.Xenia.Models.ContentDetection;
using RevSharp.Xenia.Reflection;
using RevoltClient = RevSharp.Core.Client;

namespace RevSharp.Xenia.Modules;

[RevSharpModule]
public class ContentDetectionController : BaseModule
{
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
        if (serverConfig == null || serverConfig.ShouldAllowAnalysis())
            return;
        if (message.AuthorId == Client.CurrentUserId)
            return;

        try
        {
            var analysis = await AnalyzeMessage(message);
            var deleteMatch = serverConfig.GetMessageThresholdMatch(analysis, serverConfig.DeleteThreshold);
            var flagMatch = serverConfig.GetMessageThresholdMatch(analysis, serverConfig.FlagThreshold);
            if (deleteMatch.Majority != null)
            {
                await WriteLogThreshold(
                    serverConfig,
                    ContentDetectionModule.LogDetailReason.DeleteThresholdMet,
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
                    ContentDetectionModule.LogDetailReason.FlagThresholdMet,
                    flagMatch,
                    message);
            }
        }
        catch (Exception ex)
        {
            await ReportError(ex, message, "Failed to call RunDetection");
            await WriteLine(serverConfig, $"Failed to call RunDetection\n```\n{ex}\n```");
        }
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
    public async Task WriteLine(AnalysisServerConfig config, string content)
    {
        var channel = await Client.GetChannel(config.LogChannelId) as TextChannel;
        if (channel == null)
            return;
        await channel.SendMessage(new DataMessageSend()
        {
            Content = content
        });
    }
    public async Task WriteLogThreshold(
        AnalysisServerConfig serverConfig,
        ContentDetectionModule.LogDetailReason reason,
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
                ContentDetectionModule.LogDetailReason.DeleteThresholdMet => "Deletion threshold met",
                ContentDetectionModule.LogDetailReason.FlagThresholdMet => "Flag threshold met",
                _ => reason.ToString()
            };

            var user = await Client.GetUser(message.AuthorId, false);
            if (reason != ContentDetectionModule.LogDetailReason.Error)
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
        }
    }
}