using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Google.Cloud.Vision.V1;
using RevSharp.Core.Models;
using RevSharp.ReBot.Helpers;
using RevSharp.ReBot.Models.ContentDetection;
using RevSharp.ReBot.Reflection;

namespace RevSharp.ReBot.Modules;

[RevSharpModule]
public class ContentDetectionModule : BaseModule
{
    public override bool HasHelpContent => true;
    public override string? InternalName => "condetect";
    public override string? HelpCategory => "moderation";

    public override string? HelpContent()
    {
        return string.Join("\n", new string[]
        {
            "```",
            " ------ server status --------",
            "r.condetect status        - Get content detection",
            "                          - status for this server",
            "r.condetect request       - Request content detection",
            "                          - to be enabled for this server",
            " ------- toggle server -------",
            "r.condetect disable       - Disable content detection",
            "                          - for this server",
            "r.condetect enable        - Enable content detection",
            "                          - for this server",
            "",
            " ---------- config -----------",
            "r.condetect setlogchannel  - Set current channel to",
            "                           - logging channel",
            "r.condetect thresholdset <type> <threshold> <action>",
            "                           - Set detection threshold",
            "r.condetect thresholdget   - Get thresholds",
            "",
            " ----------- help ------------",
            "r.condetect help          - Display this message",
            "r.condetect               - Display this message",
            "```"
        });
    }

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
    private async Task Command_Request(CommandInfo info, Message message)
    {
        var embed = new SendableEmbed()
        {
            Title = "Content Detection - Request Access"
        };
        
        
        var server = await message.FetchServer();
        var member = await server.GetMember(message.AuthorId);
        if (!await member.HasPermission(PermissionFlag.ManageServer))
        {
            embed.Description = $"You do not have the required permission, `ManageServer`";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }
        
        var controller = Reflection.FetchModule<ContentDetectionServerConfigController>();
        var data = await controller.Get(server.Id) ??
                   new AnalysisServerConfig()
                   {
                       ServerId = server.Id
                   };
        if (data.IsBanned)
        {
            embed.Description = $"Server is banned\nReason: `{data.BanReason}`";
            embed.Colour = "red";
        }
        else if (data.HasRequested)
        {
            embed.Description = data.AllowAnalysis switch
            {
                true => "You already have access",
                false => "Your server is still pending approval. Please wait",
            };
            embed.Colour = "yellow";
        }
        else if (data.AllowAnalysis)
        {
            embed.Description = "You already have access";
            embed.Colour = "yellow";
        }
        else
        {
            bool success = await RequestAccess(server.Id);
            if (success)
            {
                embed.Description = "Request sent. Please wait for a moderator to approve your server";
                embed.Colour = "green";
            }
            else
            {
                embed.Description = "Failed to request access";
                embed.Colour = "red";
            }
        }

        await message.Reply(embed);
    }
    private async Task<bool> RequestAccess(string serverId)
    {
        var server = await Client.GetServer(serverId);
        var controller = Reflection.FetchModule<ContentDetectionServerConfigController>();
        var data = await controller.Get(server.Id) ??
                   new AnalysisServerConfig()
                   {
                       ServerId = server.Id
                   };
        if (data.IsBanned)
            return false;
        if (data.AllowAnalysis)
            return false;

        data.HasRequested = true;
        data.AllowAnalysis = false;

        var notificationChannel = await Client.GetChannel("01H0QMAXMHXT5YF5RD61ZGK1SZ") as TextChannel;
        await notificationChannel.SendMessage(
            new SendableEmbed()
            {
                Description = string.Join(
                    "\n", new string[]
                    {
                        $"```", $"ServerId: {server.Id}", $"Name: {server.Name}", $"Members: {server.Members.Count}",
                        $"```"
                    })
            });  
        
        return true;
    }
    
    private async Task Command_ThresholdGet(CommandInfo info, Message message)
    {
        var embed = new SendableEmbed()
        {
            Title = "Content Detection - Thresholds"
        };
        
        var server = await message.FetchServer();
        var member = await server.GetMember(message.AuthorId);
        if (!await member.HasPermission(PermissionFlag.ManageServer))
        {
            embed.Description = $"You do not have the required permission, `ManageServer`";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }

        var controller = Reflection.FetchModule<ContentDetectionServerConfigController>();
        var data = await controller.Get(server.Id) ??
                   new AnalysisServerConfig()
                   {
                       ServerId = server.Id
                   };

        embed.Description = "```json\n" +
        JsonSerializer.Serialize(
            new Dictionary<string, object>()
            {
                {"Delete", data.DeleteThreshold},
                {"Flag", data.FlagThreshold}
            }, Program.SerializerOptions) + "\n```";
        await message.Reply(embed);
        
        await controller.Set(data);
    }
    private async Task Command_ThresholdSet(CommandInfo info, Message message)
    {
        var embed = new SendableEmbed()
        {
            Title = "Content Detection - Set Thresholds"
        };
        
        var server = await message.FetchServer();
        var member = await server.GetMember(message.AuthorId);
        if (!await member.HasPermission(PermissionFlag.ManageServer))
        {
            embed.Description = $"You do not have the required permission, `ManageServer`";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }
        
        if (info.Arguments.Count < 4)
        {
            embed.Description = string.Join(
            "\n", new string[]
            {
                "### Command Usage",
                "```",
                "r.condetect thresholdset <type> <threshold> <action>",
                "",
                "   type        [adult, spoof, medical, violence, racy]",
                "   threshold   range from 0-5 (-1 to disable)",
                "   action      [delete, flag]",
                "```"
            });
            await message.Reply(embed);
            return;
        }

        #region Parse
        string thresholdType = info.Arguments[1].ToLower();
        #region Threshold Type
        string[] validThresholdTypes = new[]
        {
            "adult", "spoof", "medical", "violence", "racy"
        };
        if (!validThresholdTypes.Contains(thresholdType))
        {
            embed.Description = $"Invalid threshold type `{thresholdType}`";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }
        #endregion

        int threshold;
        #region Threshold
        try
        {
            threshold = int.Parse(info.Arguments[2]);
        }
        catch (FormatException formatException)
        {
            embed.Description = $"Failed to parse threshold. `{formatException.Message}`";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }
        catch (OverflowException overflowException)
        {
            embed.Description = $"Failed to parse threshold. `{overflowException.Message}`";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }
        
        if (threshold is < -1 or > 5)
        {
            embed.Description = $"Threshold must be `>= -1` and `<= 5`. But got `{threshold}`";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }
        #endregion
        
        string targetAction = info.Arguments[3].Trim().ToLower();
        #region Action
        string[] validActions = new[]
        {
            "delete", "flag"
        };
        if (!validActions.Contains(targetAction))
        {
            embed.Description = $"Invalid action `{targetAction}`";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }
        #endregion
        #endregion
        
        
        var controller = Reflection.FetchModule<ContentDetectionServerConfigController>();
        var data = await controller.Get(server.Id) ??
           new AnalysisServerConfig()
           {
               ServerId = server.Id
           };

        ConfigThreshold? targetThreshold = targetAction switch
        {
            "delete" => data.DeleteThreshold,
            "flag" => data.FlagThreshold,
            _ => null
        };
        if (targetThreshold == null)
        {
            embed.Description = "targetThreshold is null. Aborting";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }

        // set threshold value manually since this isn't
        // javascript and we can't trust user input
        // for some weird-ass System.Reflection or JSON
        // (de|)serialization
        switch (thresholdType)
        {
            case "adult":
                targetThreshold.Adult = threshold;
                break;
            case "spoof":
                targetThreshold.Spoof = threshold;
                break;
            case "medical":
                targetThreshold.Medical = threshold;
                break;
            case "violence":
                targetThreshold.Violence = threshold;
                break;
            case "racy":
                targetThreshold.Racy = threshold;
                break;
        }

        // manually set the threshold fields depending
        // on the target action that we parsed.
        switch (targetAction)
        {
            case "delete":
                data.DeleteThreshold = targetThreshold;
                break;
            case "flag":
                data.FlagThreshold = targetThreshold;
                break;
        }

        try
        {
            await controller.Set(data);
        }
        catch (Exception e)
        {
            embed.Description = $"Failed to save data\n```{e}\n```";
            embed.Colour = "red";
            await message.Reply(embed);
            Log.Error(e);
            return;
        }

        embed.Description = "Saved Threshold Data";
        embed.Colour = "greed";
        await message.Reply(embed);
    }
    private async Task Command_SetLogChannel(CommandInfo info, Message message)
    {
        var embed = new SendableEmbed()
        {
            Title = "Content Detection - Set Log Channel"
        };
        
        var server = await message.FetchServer();
        var member = await server.GetMember(message.AuthorId);
        if (!await member.HasPermission(PermissionFlag.ManageServer))
        {
            embed.Description = $"You do not have the required permission, `ManageServer`";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }

        var controller = Reflection.FetchModule<ContentDetectionServerConfigController>();
        var data = await controller.Get(server.Id) ??
                   new AnalysisServerConfig()
                   {
                       ServerId = server.Id
                   };
        data.ServerId = server.Id;
        data.LogChannelId = message.ChannelId;
        try
        {
            await controller.Set(data);
        }
        catch (Exception e)
        {
            embed.Description = $"Failed to set log channel\n```\n{e}\n```";
            embed.Colour = "red";
            await message.Reply(embed);
            Log.Error(e);
            return;
        }
        embed.Description = $"Set Log channel to <#{message.ChannelId}>";
        await message.Reply(embed);
    }
    
    public override async Task MessageReceived(Message message)
    {
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
        if (server == null || server?.Id != "01G5XEAEJSEXKXCNTF8E5B004A")
            return;
        if (message.AuthorId == Client.CurrentUserId)
            return;

        var analysis = await AnalyzeMessage(message);
        
    }

    public async Task<AnalysisResult?> AnalyzeMessage(Message message)
    {
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

        var taskList = new List<Task>();
        if (message.Attachments != null)
        {
            foreach (var file in message.Attachments)
            {
                var url = $"{Client.EndpointNodeInfo.Features.Autumn.Url}/{file.Tag}/{file.Id}/{file.Filename}";
                taskList.Add(new Task(delegate
                {
                    ProcessUrl(url).Wait();
                }));
            }
        }

        foreach (var i in taskList)
            i.Start();
        await Task.WhenAll(taskList);
        
        if (result.Total < 1)
            return result;
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

        return result;
    }
}
