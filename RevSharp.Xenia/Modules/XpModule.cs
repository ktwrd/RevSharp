using System.Text.Json;
using System.Xml.Schema;
using RevSharp.Core;
using RevSharp.Core.Models;
using RevSharp.Xenia.Controllers;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Models;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Modules;

[RevSharpModule]
public class XpModule : CommandModule
{
    public override async Task CommandReceived(CommandInfo info, Message message)
    {
        string action = "help";
        if (info.Arguments.Count > 0)
            action = info.Arguments[0];

        switch (action)
        {
            case "help":
                await Command_Help(info, message);
                break;
            case "profile":
                await Command_Profile(info, message);
                break;
            case "setchannel":
                await Command_SetChannel(info, message);
                break;
            case "setxp":
                await Command_SetXp(info, message);
                break;
            case "disable":
                await Command_Disable(info, message);
                break;
            case "enable":
                await Command_Enable(info, message);
                break;
            case "leaderboard":
                await Command_Leaderboard(info, message);
                break;
            default:
                await Command_Help(info, message);
                break;
        }
    }

    public async Task Command_Leaderboard(CommandInfo info, Message message)
    {
        string serverId = (await message.FetchServer())?.Id ?? "";
        if (info.Arguments.Count > 1)
            serverId = info.Arguments[1].ToUpper();
        await message.Reply(await GenerateServerLeaderboard(serverId));
    }

    public async Task Command_Disable(CommandInfo info, Message message)
    {
        var server = await message.FetchServer();
        if (server == null)
        {
            await message.Reply(ServerOnlyMessage);
            return;
        }

        var embed = new SendableEmbed()
        {
            Title = "Xp System - Disable"
        };
        var member = await server.GetMember(message.AuthorId, false);
        if (!await member.HasPermission(PermissionFlag.ManageServer, forceUpdate: false))
        {
            embed.Description = $"You do not have the required permission, `ManageServer`";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }
        
        var configController = Reflection.FetchModule<LevelSystemServerConfigController>();
        if (configController == null)
        {
            embed.Colour = CommandHelper.ErrorColor;
            embed.Description = $"Failed to get config controller (is null)";
            await message.Reply(embed);
            await ReportError(new Exception($"LevelSystemServerConfigController is null (server: {server?.Id}"), message);
            return;
        }
        var data = await configController.Get(server.Id) ??
                   new LevelSystemServerConfigModel()
                   {
                       ServerId = server.Id
                   };

        data.Enable = false;
        try
        {
            await configController.Set(data);
        }
        catch (Exception ex)
        {
            embed.Colour = CommandHelper.ErrorColor;
            embed.Description = $"Failed to save data `{ex.Message}`";
            await message.Reply(embed);
            await ReportError(ex, message, $"Failed to save data for server {server.Id}");
            return;
        }
        
        embed.Description = "Success";
        embed.Colour = CommandHelper.DefaultColor;
        await message.Reply(embed);
    }
    public async Task Command_Enable(CommandInfo info, Message message)
    {
        var server = await message.FetchServer();
        if (server == null)
        {
            await message.Reply(ServerOnlyMessage);
            return;
        }

        var embed = new SendableEmbed()
        {
            Title = "Xp System - Enable"
        };
        var member = await server.GetMember(message.AuthorId, false);
        if (!await member.HasPermission(PermissionFlag.ManageServer, forceUpdate: false))
        {
            embed.Description = $"You do not have the required permission, `ManageServer`";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }
        
        var configController = Reflection.FetchModule<LevelSystemServerConfigController>();
        if (configController == null)
        {
            embed.Colour = CommandHelper.ErrorColor;
            embed.Description = $"Failed to get config controller (is null)";
            await message.Reply(embed);
            await ReportError(new Exception($"LevelSystemServerConfigController is null (server: {server?.Id}"), message);
            return;
        }
        var data = await configController.Get(server.Id) ??
                   new LevelSystemServerConfigModel()
                   {
                       ServerId = server.Id
                   };

        data.Enable = true;
        try
        {
            await configController.Set(data);
        }
        catch (Exception ex)
        {
            embed.Colour = CommandHelper.ErrorColor;
            embed.Description = $"Failed to save data `{ex.Message}`";
            await message.Reply(embed);
            await ReportError(ex, message, $"Failed to save data for server {server.Id}");
            return;
        }

        embed.Description = "Success";
        embed.Colour = CommandHelper.DefaultColor;
        await message.Reply(embed);
    }
    
    public async Task Command_Profile(CommandInfo info, Message message)
    {
        if (info.Arguments.Count > 1 && info.Arguments[1].ToLower() == "global")
        {
            await message.Reply(await GetProfile(message.AuthorId));
            return;
        }
        var server = await message.FetchServer();
        if (server == null)
        {
            await message.Reply(ServerOnlyMessage);
            return;
        }
        await message.Reply(await GetProfile(message.AuthorId, server.Id));
    }

    public async Task Command_SetXp(CommandInfo info, Message message)
    {
        if (!Reflection.Config.OwnerUserIds.Contains(message.AuthorId))
        {
            await message.Reply("This command can only be used by the bot owner");
            return;
        }
        await Task.Delay(500);
        if (info.Arguments.Count < 2)
            return;
        var server = await message.FetchServer();

        int amount = int.Parse(info.Arguments[1]);
        
        var controller = Reflection.FetchModule<LevelSystemController>();
        var data = await controller.Get(message.AuthorId, server.Id);
        await controller.GrantXp(data, message, amount);
        var asdasd = await controller.Get(message.AuthorId, server.Id);
        await message.Reply("ok\n```\n" + JsonSerializer.Serialize(asdasd, Program.SerializerOptions) + "\n```");
    }
    public async Task Command_SetChannel(CommandInfo info, Message message)
    {
        var server = await message.FetchServer();
        if (server == null)
        {
            await message.Reply(ServerOnlyMessage);
            return;
        }

        var embed = new SendableEmbed()
        {
            Title = "Xp System - Set Notification Channel"
        };
        var member = await server.GetMember(message.AuthorId, false);
        if (!await member.HasPermission(PermissionFlag.ManageServer, forceUpdate: false))
        {
            embed.Description = $"You do not have the required permission, `ManageServer`";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }

        string targetChannelId = message.ChannelId;
        if (info.Arguments.Count > 1)
        {
            var atm = CommandHelper.FindChannelId(info.Arguments[1]);
            if (atm == null)
            {
                embed.Description = $"Invalid channel argument \"{info.Arguments[1]}\" provided";
                embed.Colour = "red";
                await message.Reply(embed);
                return;
            }

            targetChannelId = atm;
        }

        targetChannelId = targetChannelId.ToUpper();

        try
        {
            var targetChannel = await Client.GetChannel(targetChannelId) as TextChannel;
            if (targetChannel == null)
            {
                embed.Description = $"Failed to fetch target channel";
                embed.Colour = "red";
                await message.Reply(embed);
                return;
            }

            if (!await targetChannel.HasPermission(Client.CurrentUser, PermissionFlag.ViewChannel))
            {
                embed.Description = "I don't have permission to view that channel!";
                embed.Colour = "red";
                await message.Reply(embed);
                return;
            }
            
        }
        catch (RevoltException rex)
        {
            Log.Error($"Failed to fetch target channel {targetChannelId}\n{rex}");
            if (rex.Message == "NotFound")
            {
                embed.Description = "Channel not found";
                embed.Colour = "red";
            }
            else
            {
                embed.Description = $"Failed to fetch target channel.\nReason: `{rex.Message}`";
                embed.Colour = "red";
            }
            await message.Reply(embed);
            return;
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to fetch target channel {targetChannelId}\n{ex}");
            embed.Description = string.Join(
                "\n", new string[]
                {
                    "Failed to fetch target channel", "```", ex.ToString(), "```"
                });
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }

        var controller = Reflection.FetchModule<LevelSystemServerConfigController>();
        var data = await controller.Get(server.Id)
            ?? new LevelSystemServerConfigModel()
            {
                ServerId = server.Id
            };
        try
        {
            await controller.Set(data);
            data.LogChannelId = targetChannelId.ToUpper();
            await controller.Set(data);
        }
        catch (Exception ex)
        {
            embed.Colour = "red";
            embed.Description = string.Join(
                "\n", new string[]
                {
                    "Failed to save to database", "```", ex.Message, ex.StackTrace, "```"
                });
            await message.Reply(embed);
            return;
        }

        embed.Description = $"Set level-up notification channel to <#{data.LogChannelId}>";
        embed.Colour = "green";
        await message.Reply(embed);
    }
    public static SendableEmbed ServerOnlyMessage => new SendableEmbed()
    {
        Title = "Xp System",
        Description = "The Experience System can only be used in servers.",
        Colour = "red"
    };

    public async Task Command_Help(CommandInfo info, Message message)
    {
        await message.Reply(
            new SendableEmbed()
            {
                Title = "Xp System - Help",
                Description = HelpContent()
            });
    }

    public async Task<SendableEmbed> GenerateServerLeaderboard(string serverId)
    {
        var embed = new SendableEmbed()
        {
            Title = "Xp System - Server Leaderboard"
        };
        var controller = Reflection.FetchModule<LevelSystemController>();
        if (controller == null)
        {
            embed.Description = $"Could not fetch LevelSystemController";
            embed.Colour = CommandHelper.ErrorColor;
            await ReportError(new Exception("Failed to fetch LevelSystemController"));
            return embed;
        }
        var data = await controller.GetServer(serverId);
        if (data == null || data.Length < 1)
        {
            embed.Description = "No records found";
            embed.Colour = CommandHelper.ErrorColor;
            return embed;
        }

        var tableLines = new List<string>()
        {
            "| Rank | Member | Level |",
            "| ---- | ------ | ----- |"
        };

        var sorted = data.OrderByDescending(v => v.Xp).ToArray();
        var length = Math.Min(5, sorted.Length);
        string[] rankText = new string[]
        {
            ":1st_place_medal:",
            ":2nd_place_medal:",
            ":3rd_place_medal:",
            ":four:",
            ":five:"
        };
        for (int i = 0; i < length; i++)
        {
            var item = sorted[i];
            var details = XpHelper.Generate(item.Xp);
            tableLines.Add("| " + string.Join(" | ", new string[]
            {
                rankText[i],
                $"<@{item.UserId}>",
                $"{item.Xp}xp, level {details.UserLevel}"
            }) + " |");
        }

        embed.Description = string.Join("\n", tableLines);
        return embed;
    }

    public async Task<SendableEmbed> GetProfile(string userId, string serverId)
    {
        try
        {
            var controller = Reflection.FetchModule<LevelSystemController>();
            var data = await controller.Get(userId, serverId) ?? new LevelMemberModel();
            var metadata = XpHelper.Generate(data);
            return new SendableEmbed()
            {
                Title = "Xp System - Profile",
                Description = string.Join(
                    "\n", new string[]
                    {
                        $"**XP**: {data?.Xp ?? 0}",
                        $"**Progress**: {Math.Round(metadata.NextLevelProgress * 100, 3)}% ({metadata.UserXp - metadata.CurrentLevelStart}/{metadata.CurrentLevelSize})",
                        $"**Level**: {metadata.UserLevel}"
                    })
            };
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to get profile for user {userId} (server: {serverId})\n{ex}");
            return new SendableEmbed()
            {
                Title = "Xp System - Profile",
                Description = string.Join(
                    "\n", new string[]
                    {
                        "Failed to fetch profile", $"`{ex.Message}`"
                    })
            };
        }
    }

    public async Task<SendableEmbed> GetProfile(string userId)
    {
        try
        {
            var controller = Reflection.FetchModule<LevelSystemController>();
            var data = await controller.GetUser(userId) ?? Array.Empty<LevelMemberModel>();
            ulong xp = 0;
            foreach (var item in data)
                xp += item.Xp;
            var metadata = XpHelper.Generate(xp);
            return new SendableEmbed()
            {
                Title = "Xp System - Global Profile",
                Description = string.Join(
                    "\n", new string[]
                    {
                        $"**XP**: {xp}",
                        $"**Progress**: {Math.Round(metadata.NextLevelProgress * 100, 3)}% ({metadata.UserXp - metadata.CurrentLevelStart}/{metadata.CurrentLevelSize})",
                        $"**Level**: {metadata.UserLevel}"
                    })
            };
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to get profile for user {userId}\n{ex}");
            return new SendableEmbed()
            {
                Title = "Xp System - Profile",
                Description = string.Join(
                    "\n", new string[]
                    {
                        "Failed to fetch profile", $"`{ex.Message}`"
                    })
            };
        }
    }

    public override string? HelpContent()
    {
        return XeniaHelper.GenerateHelp(this, new List<(string, string)>()
        {
            ("help", "display this message"),
            ("profile", "get xp profile"),
            ("setchannel", "set the current channel for level-up messages"),
            ("enable", "Enable XP System on this server."),
            ("disable", "Disable XP System on this server."),
            ("leaderboard [serverId]", "get leaderboard for the current server if none specified. will display the top 5")
        });
    }
    public override bool HasHelpContent => true;
    public override string? HelpCategory => null;
    public override string? BaseCommandName => "xp";
    public override bool WaitForInit => false;
}