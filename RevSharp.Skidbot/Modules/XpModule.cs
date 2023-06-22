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
            await message.Reply(
                new SendableEmbed()
                {
                    Title = "Xp System",
                    Description = "The Experience System can only be used in servers.",
                    Colour = "red"
                });
            return;
        }
        await message.Reply(await GetProfile(message.AuthorId, server.Id));
    }

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
                $"{r} help      - display this message",
                $"{r} profile   - Get XP profile"
            });
    }
    public override bool HasHelpContent => false;
    public override string? InternalName => "other";
    public override string? HelpCategory => "xp";
    public override string? BaseCommandName => "xp";
    public override bool WaitForInit => false;
}