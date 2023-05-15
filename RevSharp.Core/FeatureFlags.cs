using System.Diagnostics;
using System.Text.RegularExpressions;

namespace RevSharp.Core;

/// <summary>
/// Used to centralize usage of <see cref="Environment.GetEnvironmentVariable"/> so it's not scattered around the codebase.
/// </summary>
internal static class FeatureFlags
{
    private static bool ParseBool(string environmentKey, bool defaultValue)
    {
        var item = Environment.GetEnvironmentVariable(environmentKey)
            ?? $"{defaultValue}";
        item = item.ToLower().Trim();
        return item == "true";
    }

    private static string ParseString(string environmentKey, string defaultValue)
    {
        return Environment.GetEnvironmentVariable(environmentKey) ?? defaultValue;
    }

    private static int ParseInt(string envKey, int defaultValue)
    {
        var item = Environment.GetEnvironmentVariable(envKey) ?? defaultValue.ToString();
        var regex = new Regex(@"([0-9]+)");
        if (regex.IsMatch(item))
        {
            var match = regex.Match(item);
            var target = match.Groups[1].Value;
            return int.Parse(target);
        }
        Log.Warn($"Failed to parse {envKey} as integer (regex failed. value is \"{item}\"");
        return defaultValue;
    }

    internal static bool WebsocketDebugLogging => ParseBool("REVSHARP_DEBUG_WSLOG", false);
    internal static bool EnableLogColor => ParseBool("REVSHARP_LOG_COLOR", true);

    internal static LogFlag LogFlags
    {
        get
        {
            var item = (LogFlag)ParseInt("REVSHARP_LOGFLAG", (int)LogFlag.Information);
            return item;
        }
    }
}
