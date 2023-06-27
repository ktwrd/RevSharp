using RevSharp.Core.Models;
using RevSharp.Xenia.Controllers;
using RevSharp.Xenia.Helpers;
using RevSharp.Xenia.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevSharp.Xenia.Modules
{
    [RevSharpModule]
    public class InfoModule : CommandModule
    {
        public override async Task CommandReceived(CommandInfo info, Message message)
        {
            var statMod = Reflection.FetchModule<StatisticController>();
            var defaultValue = "<unknown>";
            var embed = new SendableEmbed()
            {
                Title = "Xenia Info",
                Description = string.Join("\n", new string[]
                {
                    "Heya I'm Skid, a general-purpose Revolt Boy made by [kate](https://kate.pet).",
                    "If you are having any issues with Skid, don't hesitate to ask for help in [my revolt server](https://r.kate.pet/revolt) or open an issue on [our github](https://r.kate.pet/xeniaissues)",
                    "",
                    "Xenia for Revolt was made proudly with [RevSharp](https://r.kate.pet/revsharp)!",
                    "",
                    "### Statistics",
                    $"`Guilds:     {statMod?.ServerCount.ToString() ?? defaultValue}`",
                    $"`Uptime:     {Program.GetUptimeString()}`",
                    $"`Version:    {Program.Version}`",
                    $"`Build Date: {Program.VersionDate}`",
                    "[View on Grafana](https://r.kate.pet/xeniastats)"
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
                $"{pfx}      - View information about Xenia",
                "```",
            });
        }

        public override bool HasHelpContent => true;
        public override string? HelpCategory => null;
        public override string? BaseCommandName => "info";
        public override bool WaitForInit => false;
    }
}
