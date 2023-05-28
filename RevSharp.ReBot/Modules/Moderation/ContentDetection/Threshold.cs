using System.Text.Json;
using RevSharp.Core.Models;
using RevSharp.ReBot.Helpers;
using RevSharp.ReBot.Models.ContentDetection;

namespace RevSharp.ReBot.Modules;

public partial class ContentDetectionModule
{
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
            var p = Program.ConfigData.Prefix;
            embed.Description = string.Join(
            "\n", new string[]
            {
                "### Command Usage",
                "```",
                $"{p}condetect thresholdset <type> <threshold> <action>",
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
}