using System.Text.RegularExpressions;
using RevSharp.Core.Models;
using RevSharp.Skidbot.Reflection;

namespace RevSharp.Skidbot.Helpers;

public class CommandInfo
{
    public string Command { get; set; }
    public List<string> Arguments { get; set; }
    public string Content { get; set; }
}
public static class CommandHelper
{
    public static CommandInfo? FetchInfo(string prefix, string content)
    {
        if (!content.StartsWith(prefix))
            return null;
        
        var substr = content.Substring(prefix.Length);
        var args = substr.Split(" ").ToList();
        var cmd = args[0];
        // if no command and it's "r." then we ignore
        if (args.Count < 1)
            return null;
        
        // remove cmd
        args.RemoveAt(0);
        return new CommandInfo()
        {
            Command = cmd,
            Arguments = args,
            Content = content
        };
    }

    public static CommandInfo? FetchInfo(ReflectionInclude include, Message message)
    {
        return FetchInfo(include.Config.Prefix, message.Content ?? "");
    }

    public static CommandInfo? FetchInfo(BaseModule module, Message message)
    {
        return FetchInfo(module.Reflection.Config.Prefix, message.Content ?? "");
    }

    public static string? FindChannelId(string content)
    {
        var channelIdRegex = new Regex(@"^([0-9A-Za-z]{26})$");
        if (channelIdRegex.IsMatch(content))
            return content;

        var channelMentionRegex = new Regex(@"^<#([0-9A-Za-z]{26})>$");
        var channelMentionMatch = channelMentionRegex.Match(content);
        if (channelMentionMatch != null)
        {
            if (channelMentionMatch.Groups.Count > 1)
            {
                return channelMentionMatch.Groups[1].Value.ToString();
            }
        }

        return null;
    }
    public static bool IsValidUlid(string content)
    {
        var ulidRegex = new Regex(@"^([0-9A-Za-z]{26})$");
        return ulidRegex.IsMatch(content);
    }
    public static string FetchContent(CommandInfo info, int deleteBefore = 1)
    {
        var content = new List<string>();
        for (int i = 0; i < info.Arguments.Count; i++)
            if (i > deleteBefore)
                content.Add(info.Arguments[i]);
        return string.Join(' ', content);
    }
}