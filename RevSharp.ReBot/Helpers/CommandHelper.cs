using RevSharp.Core.Models;

namespace RevSharp.ReBot.Helpers;

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

    public static CommandInfo? FetchInfo(Message message)
    {
        return FetchInfo(Program.Config.Prefix, message.Content ?? "");
    }
}