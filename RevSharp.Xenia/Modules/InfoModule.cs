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
        private string GetContent()
        {
            var statMod = Reflection.FetchModule<StatisticController>();
            var defaultValue = "<unknown>";
            string content = ResourceHelper.GetAsString(GetType().Assembly, "RevSharp.Xenia.Data.info-description.md").ReplaceLineEndings("\n");
            var replacePairs = new Dictionary<string, string>()
            {
                {"serverCount", statMod?.ServerCount.ToString() ?? defaultValue},
                {"uptime", Program.GetUptimeString()},
                {"version", Program.Version},
                {"versionDate", Program.VersionDate.ToString()},
                {"latency", Client.WSLatency.ToString()},
            };
            foreach (var pair in replacePairs)
            {
                content = content.Replace("$" + pair.Key + "$", pair.Value);
            }

            return content;
        }
        public override async Task CommandReceived(CommandInfo info, Message message)
        {
            string action = "";
            if (info.Arguments.Count > 0)
                action = info.Arguments[0].ToLower();
            var embed = new SendableEmbed()
            {
                Title = "Xenia Info"
            };
            switch (action)
            {
                case "modules":
                    var plugins = Reflection.GetPlugins(includeVersion: true);
                    embed.Title += " - Modules";
                    embed.Description = string.Join(
                        "\n", new string[]
                        {
                            "| Name | Version |", "| - | - |",
                            string.Join("\n", plugins.Select(v => "| " + v.Replace(" ", " | ") + " |"))
                        });
                    break;
                default:
                    embed.Description = GetContent();
                    break;
            }
            await message.Reply(embed);
        }

        public override string? HelpContent()
        {
            var pfx = Program.ConfigData.Prefix + BaseCommandName;
            return string.Join("\n", new string[]
            {
                "```",
                $"{pfx}         - View information about Xenia",
                $"{pfx} modules - List modules incl version",
                "```",
            });
        }

        public override bool HasHelpContent => true;
        public override string? HelpCategory => null;
        public override string? BaseCommandName => "info";
        public override bool WaitForInit => false;
    }
}
