using System.Text.Json;
using RevSharp.Core.Models;
using RevSharp.Skidbot.Helpers;
using RevSharp.Skidbot.Reflection;

namespace RevSharp.Skidbot.Modules.Utility;

[RevSharpModule]
public class PermissionCloneModule : BaseModule
{
    public override string? HelpCategory => "utility";
    public override string? InternalName => "permissionclone";
    public override string? BaseCommandName => "permissionclone";
    public override bool HasHelpContent => true;

    public override string HelpContent()
    {
        var p = Program.ConfigData.Prefix + InternalName;
        return string.Join(
            "\n", new string[]
            {
                "```",
                $"{p} channel <source channel> ...<target channels>",
                $"{p} role <source role id> ...<target role ids>",
                "```"
            });
    }

    public class ArgumentParseResult
    {
        public int[] InvalidIndexes { get; set; }
        public string[] Result { get; set; }
        public string[] ParsedArguments { get; set; }
    }
    private ArgumentParseResult ParseSnowflakeIds(
        CommandInfo info,
        Func<string, bool> validateFunc,
        string defaultValue = "",
        int argumentOffset = 1,
        string[]? defaultAlias = null)
    {
        defaultAlias ??= Array.Empty<string>();

        var resultList = new List<string>();
        var invalidIndexes = new List<int>();
        
        for (int i = argumentOffset; i < info.Arguments.Count; i++)
        {
            var arg = info.Arguments[i].ToLower();
            if (defaultAlias.Contains(arg))
            {
                resultList.Add(defaultValue);
            }
            else
            {
                var res = validateFunc(arg);
                if (res)
                    resultList.Add(arg);
                else
                    invalidIndexes.Add(i);
            }
        }

        return new ArgumentParseResult()
        {
            InvalidIndexes = invalidIndexes.ToArray(),
            Result = resultList.ToArray(),
            ParsedArguments = info.Arguments.ToArray()
        };
    }
    
    private async Task Command_PermissionCloneChannel(CommandInfo info, Message message)
    {
        var embed = new SendableEmbed()
        {
            Title = "Permission Cloning"
        };
        
        string validValueHelp = string.Join("\n", new string[]
        {
            "Valid Values;",
            "```",
            "[current|this|default] - Current channel",
            "<#XXXXX>               - Mentioned channel",
            "XXXXXX                 - Channel Id",
            "```"
        });
        
        string sourceChannelId = "";
        var parsedArgs = ParseSnowflakeIds(
            info, (v) =>
            {
                return CommandHelper.FindChannelId(v.ToUpper()) != null;
            }, message.ChannelId, defaultAlias: new string[]
            {
                "current", "this", "default"
            });

        if (parsedArgs.Result.Length < 1)
        {
            embed.Description = string.Join(
                "\n", new string[]
                {
                    "No valid arguments found!", "", validValueHelp
                });
            await message.Reply(embed);
            return;
        }
        /*for (int i = 0; i < info.Arguments.Count; i++)
        {
            var arg = info.Arguments[i].ToLower();
            if (i == 1)
            {
                if (arg == "current" || arg == "this")
                {
                    sourceChannelId = message.ChannelId;
                }
                else
                {
                    var found = CommandHelper.FindChannelId(arg.ToUpper());
                    if (found != null)
                        sourceChannelId = found;
                }
            }

            if (i > 1)
            {
                var found = CommandHelper.FindChannelId(arg.ToUpper());
                if (found != null)
                    targetChannelIds.Add(found);
            }
        }*/
        for (int i = 0; i < parsedArgs.Result.Length; i++)
            parsedArgs.Result[i] = CommandHelper.FindChannelId(parsedArgs.Result[i].ToUpper()) ?? "";

        // set sourceChannelId if the index isn't in InvalidIndexes
        if (!parsedArgs.InvalidIndexes.Contains(1))
        {
            sourceChannelId = parsedArgs.Result[0];
        }

        if (sourceChannelId.Length < 1)
        {
            embed.Description = string.Join(
                "\n", new string[]
                {
                    "Invalid/Missing source channel.",
                    "",
                }) + validValueHelp;
            await message.Reply(embed);
            return;
        }
        
        // set to everything but the sourceChannelId
        var targetChannelIds = parsedArgs.Result.ToList().GetRange(1, parsedArgs.Result.Length - 1);

        if (targetChannelIds.Count < 1)
        {
            embed.Description = string.Join(
                "\n", new string[]
                {
                    "No target channels provided. Everything after the source channel is a target channel.",
                    "It can be the Channel Id directly, or the mentioned channel."
                });
            await message.Reply(embed);
            return;
        }

        var sourceChannel_raw = await Client.GetChannel(sourceChannelId, true);
        var sourceChannel = sourceChannel_raw as IServerChannel;
        if (sourceChannel == null)
        {
            embed.Description = string.Join(
                "\n", new string[]
                {
                    "Failed to fetch the Source Channel! Please make sure that I have access to it."
                });
            await message.Reply(embed);
            return;
        }
        
        var server = await Client.GetServer(sourceChannel.ServerId);
        var highestBotRole = await server.GetHighestMemberRole(Client.CurrentUserId);

        int failCount = 0;
        int totalCount = 0;
        foreach (var channelId in targetChannelIds)
        {
            totalCount++;
            var targetChannel = await Client.GetChannel(channelId, true) as IServerChannel;
            if (targetChannel == null)
            {
                totalCount++;
                failCount++;
                continue;
            }

            await targetChannel.Fetch();
        
            foreach (var sourcePair in sourceChannel.RolePermissions)
            {
                var canAccess = await server.CanMemberAccessRole(Client.CurrentUserId, sourcePair.Key);
                if (canAccess == null || canAccess == false)
                {
                    Log.Debug($"Ignoring {sourcePair.Key}");
                    continue;
                }
                if (!targetChannel.RolePermissions.ContainsKey(sourcePair.Key) ||
                    (targetChannel.RolePermissions.TryGetValue(sourcePair.Key, out PermissionCompare value) &&
                     value != sourcePair.Value))
                {
                    bool res = false;
                    try
                    {
                        res = await targetChannel.SetRolePermission(
                            sourcePair.Key, sourcePair.Value.Allow, sourcePair.Value.Deny);
                    }
                    catch (Exception ex)
                    {
                        embed.Description = string.Join(
                            "\n", new string[]
                            {
                                $"Failed to set permissions for <#{targetChannel.Id}>", $"```", ex.Message, "```"
                            });
                        await message.Reply(embed);
                        return;
                    }
                    totalCount++;
                    failCount +=  res ?
                        0 :
                        1;
                }
            }

            var sourceDefault = sourceChannel.DefaultPermissions ?? new PermissionCompare();
            var targetDefault = targetChannel.DefaultPermissions ?? new PermissionCompare();

            if (targetDefault.Allow != sourceDefault.Allow ||
                targetDefault.Deny != sourceDefault.Deny)
            {
                totalCount++;
                failCount += await targetChannel.SetDefaultPermission(
                    sourceDefault.Allow,
                    sourceDefault.Deny) ?
                    0 :
                    1;
            }
        }

        embed.Description = string.Join(
            "\n", new string[]
            {
                $"Processed {targetChannelIds.Count} channels ({failCount}/{totalCount} failures)"
            });
        await message.Reply(embed);
    }

    private async Task Command_Role(CommandInfo info, Message message)
    {
        
    }
    
    private async Task Command_Help(CommandInfo info, Message message)
    {
        var embed = new SendableEmbed()
        {
            Title = "Permission Cloning - Help"
        };
        embed.Description = HelpContent();
        await message.Reply(embed);
    }

    public override async Task CommandReceived(CommandInfo info, Message message)
    {
        if (info?.Command != InternalName)
            return;

        var action = "help";
        if (info.Arguments.Count > 0)
            action = info.Arguments[0].ToLower();
        switch (action)
        {
            case "channel":
                await Command_PermissionCloneChannel(info, message);
                break;
            default:
                await Command_Help(info, message);
                break;
            case "help":
                await Command_Help(info, message);
                break;
        }
    }
}