using System.Text.Json;
using System.Xml.Schema;
using RevSharp.Core;
using RevSharp.Core.Models;
using RevSharp.Skidbot.Controllers;
using RevSharp.Skidbot.Helpers;
using RevSharp.Skidbot.Models;
using RevSharp.Skidbot.Reflection;

namespace RevSharp.Skidbot.Modules;

[RevSharpModule]
public class XpModule : BaseModule
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
            /*case "setxp":
                await Command_SetXp(info, message);
                break;*/
            default:
                await Command_Help(info, message);
                break;
        }
    }

    public async Task Command_Profile(CommandInfo info, Message message)
    {
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
        await Task.Delay(1000);
        if (info.Arguments.Count < 2)
            return;
        var server = await message.FetchServer();

        int amount = int.Parse(info.Arguments[1]);
        
        var controller = Reflection.FetchModule<LevelSystemController>();
        var data = await controller.Get(message.AuthorId, server.Id);
        await controller.GrantXp(data, message, amount);
        var asdasd = await controller.Get(message.AuthorId, server.Id);
        message.Reply("ok\n```\n" + JsonSerializer.Serialize(asdasd, Program.SerializerOptions) + "\n```");
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
                        $"**Progress**: {Math.Round(metadata.NextLevelProgress * 100, 3)}% ({metadata.UserXp - metadata.CurrentLevelStart}/{metadata.CurrentLevelEnd})",
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

    public override string? HelpContent()
    {
        var r = Program.ConfigData.Prefix + BaseCommandName;
        return string.Join(
            "\n", new string[]
            {
                "```",
                $"{r} help       - display this message",
                $"{r} profile    - Get XP profile",
                $"{r} setchannel - Set channel for level-up messages"
            });
    }
    public override bool HasHelpContent => false;
    public override string? InternalName => "other";
    public override string? HelpCategory => "xp";
    public override string? BaseCommandName => "xp";
    public override bool WaitForInit => false;
}