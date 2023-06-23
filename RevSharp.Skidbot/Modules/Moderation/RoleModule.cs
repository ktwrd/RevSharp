using RevSharp.Core;
using RevSharp.Core.Models;
using RevSharp.Skidbot.Helpers;
using RevSharp.Skidbot.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevSharp.Skidbot.Modules
{
    [RevSharpModule]
    public class RoleModule : BaseModule
    {
        public override async Task CommandReceived(CommandInfo info, Message message)
        {
            var action = "help";
            if (info.Arguments.Count > 0)
                action = info.Arguments[0].ToLower();

            switch (action)
            {
                case "setcolor":
                    await Command_SetColor(info, message);
                    break;
                case "help":
                    await Command_Help(info, message);
                    break;
                default:
                    await Command_Help(info, message);
                    break;
            }
        }
        private async Task Command_SetColor(CommandInfo info, Message message)
        {
            var server = await message.FetchServer();
            if (server == null)
            {
                await message.Reply($"This command can only be ran on servers");
                return;
            }

            if (info.Arguments.Count < 2)
            {
                await message.Reply($"Missing arguments `roleId` and `targetColor`");
                return;
            }
            else if (info.Arguments.Count < 3)
            {
                await message.Reply($"Missing argument `targetColor`");
                return;
            }
            string targetRoleId = info.Arguments[1].ToUpper();
            if (!CommandHelper.IsValidUlid(targetRoleId))
            {
                await message.Reply($"Invalid Role Id `{targetRoleId}`");
                return;
            }

            string targetColor = CommandHelper.FetchContent(info, 1);

            if (!server.Roles.ContainsKey(targetRoleId))
            {
                await message.Reply($"Role not found");
                return;
            }
            bool succ = false;
            try
            {
                succ = await server.EditRole(targetRoleId, new DataEditRole()
                {
                    Colour = targetColor
                });
            }
            catch (RevoltException rex)
            {
                await message.Reply($"Failed to edit role. `{rex.Message}`");
                return;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to edit role {targetRoleId}\n{ex}");
                await message.Reply($"Failed to edit role.\n```\n{ex}\n```");
                return;
            }

            await message.Reply(succ switch
            {
                true => $"Role color changed successfully",
                false => $"Failed to set role color"
            });
        }
        private async Task Command_Help(CommandInfo info, Message message)
        {
            await message.Reply(new SendableEmbed()
            {
                Title = "Role Moderation",
                Description = HelpContent()
            });
        }
        public override PermissionFlag? RequireServerPermission => PermissionFlag.ManageRole;
        public override string? HelpContent()
        {
            var r = Reflection.Config.Prefix + BaseCommandName;
            string GenerateHelp(string c, string d)
            {
                return string.Join("\n", new string[]
                {
                    $">`{r} {c}`",
                    $">{d}",
                    ""
                });
            }
            return string.Join("\n", new string[]
            {
                GenerateHelp(
                    "help",
                        "display this comand"),
                GenerateHelp(
                    "setcolor <roleId> <...targetColor>",
                    "set color field of role")
            });
        }
        public override bool HasHelpContent => true;
        public override string? InternalName => "mod_role";
        public override string? HelpCategory => "moderation";
        public override string? BaseCommandName => "role";
    }
}
