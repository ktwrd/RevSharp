using System.Text.Json;
using System.Text.RegularExpressions;
using RevSharp.Skidbot.Controllers;
using RevSharp.Skidbot.Helpers;
using RevSharp.Skidbot.Reflection;
using RevSharp.Core.Models;
using RevSharp.Skidbot.Reflection;

namespace RevSharp.Skidbot.Modules;

[RevSharpModule]
public class AuthentikModule : BaseModule
{
    public override string? HelpContent()
    {
        var p = Program.ConfigData.Prefix;
        return string.Join(
            "\n", new string[]
            {
                "```",
                $"{p}auth createuser user=<username>",
                "       create a user account",
                "",
                $"{p}auth deleteuser <username/id>",
                $"      delete user with supplied username/id",
                "",
                $"{p}auth resetlink <username/id>",
                "       generate password reset link for username/id",
                "",
                $"{p}auth addtogroup user=<username/id> group=<groupname/id>",
                "       add user to group",
                "",
                $"{p}auth removefromgroup user=<username/id> group=<groupname/id>",
                "       remove user from group",
                "",
                $"{p}auth help",
                "       display this message"
            });
    }

    public override bool HasHelpContent => true;
    public override string? InternalName => "auth";
    public override string? HelpCategory => "admin";
    public override string? BaseCommandName => "auth";

    public override async Task CommandReceived(CommandInfo info, Message message)
    {
        if (!Program.ConfigData.OwnerUserIds.Contains(message.AuthorId))
            return;

        if (info.Command != "auth")
            return;

        string action = " ";
        if (info.Arguments.Count > 0)
            action = info.Arguments[0].Trim().ToLower();

        var embed = new SendableEmbed()
        {
            Title = "Authentik",
            Description = ""
        };
        switch (action)
        {
            case "createuser":
                await Command_CreateUser(info, message);
                break;
            case "resetlink":
                await Command_CreateResetLink(info, message);
                break;
            case "deleteuser":
                await Command_DeleteUser(info, message);
                break;
            case "addtogroup":
                await Command_AddUserToGroup(info, message);
                break;
            case "removefromgroup":
                await Command_RemoveUserFromGroup(info, message);
                break;
            case "help":
                embed.Description = HelpContent();
                embed.Colour = "blue";
                embed.Title += " - Help";
                break;
            default:
                embed.Description = $"Unknown subcommand `{action}`\n### Usage\n{HelpContent()}";
                embed.Colour = "red";
                break;
        }

        if (embed.Description.Length > 0)
        {
            await message.Reply(embed);
        }
    }

    /// <summary>
    /// r.auth addtogroup user={username} group={id}
    /// </summary>
    private async Task Command_AddUserToGroup(CommandInfo info, Message message)
    {
        var embed = new SendableEmbed()
        {
            Title = "Authentik - Add User to Group"
        };
        var parameters = ParseArguments(info);
        var requiredArguments = new Dictionary<string, string>()
        {
            {"group", "Group Name or Id"},
            {"user", "Username or User Id"}
        };
        bool hasAll = true;
        List<string> missingParameters = new List<string>();
        foreach (var pair in requiredArguments)
        {
            if (!parameters.ContainsKey(pair.Key))
            {
                missingParameters.Add(pair.Key);
                hasAll = false;
            }
        }

        if (!hasAll)
        {
            embed.Colour = "red";
            embed.Description = "Missing required arguments;\n```\n" + string.Join("\n",
                missingParameters.Select(v => $"{v} - {requiredArguments[v]}")) + "\n```";
            await message.Reply(embed);
            return;
        }

        string? targetUser = parameters["user"];
        targetUser = await SafelyGetUserId(targetUser);
        if (targetUser == null)
        {
            embed.Colour = "red";
            embed.Description = $"User `{parameters["user"]}` not found.";
            await message.Reply(embed);
            return;
        }
        
        string? targetGroup = parameters["group"];
        targetGroup = await SafelyGetGroupId(targetGroup);
        if (targetGroup == null)
        {
            embed.Colour = "red";
            embed.Description = $"Group `{parameters["user"]}` not found.";
            await message.Reply(embed);
            return;
        }

        var controller = Reflection.FetchModule<AuthentikController>();
        
        bool success = true;
        try
        {
            success = await controller.AddToGroup(int.Parse(targetUser), targetGroup);
        }
        catch (Exception e)
        {
            embed.Description = $"```\n{e}\n```";
            await message.Reply(embed);
            return;
        }

        if (success)
        {
            embed.Description = $"Added user to group!";
            embed.Colour = "green";
        }
        else
        {
            embed.Description = $"Failed to add user to group (might be in it already)";
            embed.Colour = "orange";
        }

        await message.Reply(embed);
    }
    
    /// <summary>
    /// r.auth removefromgroup user={username} group={id}
    /// </summary>
    private async Task Command_RemoveUserFromGroup(CommandInfo info, Message message)
    {
        var embed = new SendableEmbed()
        {
            Title = "Authentik - Remove User from Group"
        };
        var parameters = ParseArguments(info);
        var requiredArguments = new Dictionary<string, string>()
        {
            {"group", "Group Name or Id"},
            {"user", "Username or User Id"}
        };
        bool hasAll = true;
        List<string> missingParameters = new List<string>();
        foreach (var pair in requiredArguments)
        {
            if (!parameters.ContainsKey(pair.Key))
            {
                missingParameters.Add(pair.Key);
                hasAll = false;
            }
        }

        if (!hasAll)
        {
            embed.Colour = "red";
            embed.Description = "Missing required arguments;\n```\n" + string.Join("\n",
                missingParameters.Select(v => $"{v} - {requiredArguments[v]}")) + "\n```";
            await message.Reply(embed);
            return;
        }

        string? targetUser = parameters["user"];
        targetUser = await SafelyGetUserId(targetUser);
        if (targetUser == null)
        {
            embed.Colour = "red";
            embed.Description = $"User `{parameters["user"]}` not found.";
            await message.Reply(embed);
            return;
        }
        
        string? targetGroup = parameters["group"];
        targetGroup = await SafelyGetGroupId(targetGroup);
        if (targetGroup == null)
        {
            embed.Colour = "red";
            embed.Description = $"Group `{parameters["user"]}` not found.";
            await message.Reply(embed);
            return;
        }

        var controller = Reflection.FetchModule<AuthentikController>();
        
        bool success = true;
        try
        {
            success = await controller.RemoveFromGroup(int.Parse(targetUser), targetGroup);
        }
        catch (Exception e)
        {
            embed.Description = $"```\n{e}\n```";
            await message.Reply(embed);
            return;
        }

        if (success)
        {
            embed.Description = $"Removed user from group!";
            embed.Colour = "green";
        }
        else
        {
            embed.Description = $"Failed to remove user from group";
            embed.Colour = "orange";
        }

        await message.Reply(embed);
    }
    
    #region User Management
    /// <summary>
    /// r.auth createuser user={username}
    /// </summary>
    /// <param name="info"></param>
    /// <param name="message"></param>
    private async Task Command_CreateUser(CommandInfo info, Message message)
    {
        var parameters = ParseArguments(info);
        var requiredArguments = new Dictionary<string, string>()
        {
            {"user", "Username for the new account"}/*,
            {"email", "Email address"}*/
        };
        bool hasAll = true;
        List<string> missingParameters = new List<string>();
        foreach (var pair in requiredArguments)
        {
            if (!parameters.ContainsKey(pair.Key))
            {
                missingParameters.Add(pair.Key);
                hasAll = false;
            }
        }

        var embed = new SendableEmbed()
        {
            Title = "Authentik - Create User"
        };
        if (!hasAll)
        {
            embed.Colour = "red";
            embed.Description = "Missing required arguments;\n```\n" + string.Join("\n",
                missingParameters.Select(v => $"{v} - {requiredArguments[v]}")) + "\n```";
            await message.Reply(embed);
            return;
        }

        var controller = Reflection.FetchModule<AuthentikController>();
        AuthentikUserResponse? response = null;
        try
        {
            response = await controller.CreateAccountAsync(parameters["user"]);
        }
        catch (Exception e)
        {
            embed.Description = $"```\n{e}\n```";
            await message.Reply(embed);
            return;
        }
        if (response == null)
        {
            embed.Colour = "red";
            embed.Description = $"idfk what happened, you gotta fix this kate. api returned null you goofy goober";
            await message.Reply(embed);
            return;
        }

        embed.Colour = "green";
        embed.Description = string.Join(
            "\n", new string[]
            {
                $"Account created! ([view](https://{Program.ConfigData.AuthentikUrl}/if/admin/#/identity/users/{response.Id};%7B%22page%22%3A%22page-overview%22%7D))",
                "```",
                $"Id: {response.Id}",
                $"Username: {response.Username}",
                "```"
            });
        await message.Reply(embed);
    }

    /// <summary>
    /// r.auth deleteuser {username/id}
    /// </summary>
    private async Task Command_DeleteUser(CommandInfo info, Message message)
    {
        var embed = new SendableEmbed()
        {
            Title = "Authentik - Delete User"
        };
        
        if (info.Arguments.Count < 2)
        {
            embed.Colour = "red";
            embed.Description = $"Must have the username/id as the 2nd argument";
            await message.Reply(embed);
            return;
        }

        var controller = Reflection.FetchModule<AuthentikController>();

        string? targetUserId = info.Arguments[1];
        var integerRegex = new Regex(@"^[0-9]+$");
        if (!integerRegex.IsMatch(targetUserId))
        {
            try
            {
                targetUserId = await SafelyGetUserId(targetUserId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                embed.Description = $"`{e.Message}`\n```\n{e}\n```";
                embed.Colour = "red";
                await message.Reply(embed);
                return;
            }
        }

        if (targetUserId == null)
        {
            embed.Description = $"No users found with the username of `{targetUserId}`";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }

        bool success = true;
        try
        {
            success = await controller.DeleteUser(targetUserId);
        }
        catch (Exception e)
        {
            embed.Description = $"`{e.Message}`\n```\n{e}\n```";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }

        if (success)
        {
            embed.Description = "Account deleted :3";
            embed.Colour = "green";
        }
        else
        {
            embed.Description = "Failed to delete user ;w;";
            embed.Colour = "red";
        }
        await message.Reply(embed);
    }
    
    /// <summary>
    /// r.auth resetlink {username/id}
    /// </summary>
    /// <param name="info"></param>
    /// <param name="message"></param>
    private async Task Command_CreateResetLink(CommandInfo info, Message message)
    {
        var embed = new SendableEmbed()
        {
            Title = "Authentik - Create Reset Link"
        };
        
        if (info.Arguments.Count < 2)
        {
            embed.Colour = "red";
            embed.Description = $"Must have the username/id as the 2nd argument";
            await message.Reply(embed);
            return;
        }

        var controller = Reflection.FetchModule<AuthentikController>();

        string? targetUserId = info.Arguments[1];
        var integerRegex = new Regex(@"^[0-9]+$");
        if (!integerRegex.IsMatch(targetUserId))
        {
            try
            {
                targetUserId = await SafelyGetUserId(targetUserId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                embed.Description = $"`{e.Message}`\n```\n{e}\n```";
                embed.Colour = "red";
                await message.Reply(embed);
                return;
            }
        }

        if (targetUserId == null)
        {
            embed.Description = $"No users found with the username of `{targetUserId}`";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }


        try
        {
            var link = await controller.CreatePasswordResetLink(targetUserId);
            if (link == null)
            {
                embed.Description = $"Failed to get reset link. It's null...";
                embed.Colour = "orange";
                await message.Reply(embed);
                return;
            }

            embed.Description = $"[Reset Link]({link})";
            embed.Colour = "blue";
            await message.Reply(embed);
            return;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            embed.Description = $"`{e.Message}`\n```\n{e}\n```";
            embed.Colour = "red";
            await message.Reply(embed);
            return;
        }
    }
    #endregion
    
    public async Task<string?> SafelyGetUserId(string param)
    {
        var controller = Reflection.FetchModule<AuthentikController>();
        var integerRegex = new Regex(@"^[0-9]+$");
        if (!integerRegex.IsMatch(param))
        {
            var searchResponse = await controller.GetUsers(param);
            if (searchResponse.Results.Length < 1)
            {
                return null;
            }
            return searchResponse.Results[0].Id.ToString();
        }

        return param;
    }

    public async Task<string?> SafelyGetGroupId(string param)
    {
        var controller = Reflection.FetchModule<AuthentikController>();
        var integerRegex = new Regex(@"^[0-9]+$");
        if (!integerRegex.IsMatch(param))
        {
            var searchResponse = await controller.GetGroups(param);
            if (searchResponse.Results.Length < 1)
            {
                return null;
            }
            return searchResponse.Results[0].Uuid;
        }

        return param;
    }
    
    private Dictionary<string, string> ParseArguments(CommandInfo info)
    {
        var dict = new Dictionary<string, string>();
        for (int i = 1; i < info.Arguments.Count; i++)
        {
            var item = info.Arguments[i];
            var iof = item.IndexOf("=");
            if (iof > 0)
            {
                var key = item.Substring(0, iof);
                var value = item.Substring(iof + 1);
                dict.Add(key, value);
            }
        }

        return dict;
    }
}