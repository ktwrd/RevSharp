using System.Text.RegularExpressions;
using RevSharp.Core.Models;
using RevSharp.Xenia.Reflection;

namespace RevSharp.Xenia.Helpers;

public class CommandInfo
{
    public string Command { get; set; }
    public List<string> Arguments { get; set; }
    public string Content { get; set; }
}
public static class CommandHelper
{
    public const string DefaultColor = "#0069d9";
    public const string ErrorColor = "#c82333";
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
        var idRegex = new Regex(@"^([0-9A-Za-z]{26})$");
        if (idRegex.IsMatch(content))
            return content;

        var mentionRegex = new Regex(@"^<#([0-9A-Za-z]{26})>$");
        var mentionMatch = mentionRegex.Match(content);
        if (mentionMatch != null)
        {
            if (mentionMatch.Groups.Count > 1)
            {
                return mentionMatch.Groups[1].Value.ToString();
            }
        }

        return null;
    }

    public static string? FindUserId(string content)
    {
        var idRegex = new Regex(@"^([0-9A-Za-z]{26})$");
        if (idRegex.IsMatch(content))
            return content;

        var mentionRegex = new Regex(@"^<@([0-9A-Za-z]{26})>$");
        var mentionMatch = mentionRegex.Match(content);
        if (mentionMatch != null)
        {
            if (mentionMatch.Groups.Count > 1)
            {
                return mentionMatch.Groups[1].Value.ToString();
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