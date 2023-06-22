using RevSharp.Core.Models;
using RevSharp.Skidbot.Controllers;
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
    public class InfoModule : BaseModule
    {
        public override async Task CommandReceived(CommandInfo info, Message message)
        {
            var statMod = Reflection.FetchModule<StatisticController>();
            var defaultValue = "<unknown>";
            var embed = new SendableEmbed()
            {
                Title = "Skidbot Info",
                Description = string.Join("\n", new string[]
                {
                    "Heya I'm Skid, a general-purpose Revolt Boy made by [kate](https://kate.pet).",
                    "If you are having any issues with Skid, don't hesitate to ask for help in [my revolt server](https://r.kate.pet/revolt) or open an issue on [our github](https://r.kate.pet/revoltskidbotissues)",
                    "",
                    "Skidbot for Revolt was made proudly with [RevSharp](https://r.kate.pet/revsharp)!",
                    "",
                    "### Statistics",
                    $"`Guilds:     {statMod?.ServerCount.ToString() ?? defaultValue}`",
                    $"`Uptime:     {Program.GetUptimeString()}`",
                    $"`Version:    {Program.Version}`",
                    $"`Build Date: {Program.VersionDate}`"
                })
            };
            await message.Reply(embed);
        }

        public override string? HelpContent()
        {
            var pfx = Program.ConfigData.Prefix + BaseCommandName;
            return string.Join("\n", new string[]
            {
                "```",
                $"{pfx}      - View information about Skidbot",
                "```",
            });
        }

        public override bool HasHelpContent => true;
        public override string? InternalName => "info";
        public override string? HelpCategory => null;
        public override string? BaseCommandName => "info";
        public override bool WaitForInit => false;
    }
}
