using System.Diagnostics;
using System.Text.RegularExpressions;

namespace RevSharp.Core;

/// <summary>
/// Used to centralize usage of <see cref="Environment.GetEnvironmentVariable"/> so it's not scattered around the codebase.
/// </summary>
internal static class FeatureFlags
{
    /// <summary>
    /// Parses an environment variable as a boolean. When trimmed&lowercased to `true` it will return true, but anything else will return `false`.
    /// When the environment variable isn't found, it wil default to <see cref="defaultValue"/>
    /// </summary>
    /// <param name="environmentKey"></param>
    /// <param name="defaultValue">Used when environment variable is not set.</param>
    /// <returns>`true` when envar is true, `false` when not true, <see cref="defaultValue"/> when not found.</returns>
    private static bool ParseBool(string environmentKey, bool defaultValue)
    {
        var item = Environment.GetEnvironmentVariable(environmentKey)
            ?? $"{defaultValue}";
        item = item.ToLower().Trim();
        return item == "true";
    }

    /// <summary>
    /// Just <see cref="Environment.GetEnvironmentVariable(string variable)"/> but when null it's <see cref="defaultValue"/>
    /// </summary>
    private static string ParseString(string environmentKey, string defaultValue)
    {
        return Environment.GetEnvironmentVariable(environmentKey) ?? defaultValue;
    }

    /// <summary>
    /// Parse an environment variable as <see cref="Int32"/>.
    /// 
    /// - Fetch Environment variable (when null, set to <see cref="defaultValue"/> as string)
    /// - Do regex match ^([0-9]+)$
    /// - When success, parse item as integer then return
    /// - When fail, return default value
    /// </summary>
    /// <returns></returns>
    private static int ParseInt(string envKey, int defaultValue)
    {
        var item = Environment.GetEnvironmentVariable(envKey) ?? defaultValue.ToString();
        item = item.Trim();
        var regex = new Regex(@"^([0-9]+)$");
        if (regex.IsMatch(item))
        {
            var match = regex.Match(item);
            var target = match.Groups[1].Value;
            return int.Parse(target);
        }
        Log.Warn($"Failed to parse {envKey} as integer (regex failed. value is \"{item}\"");
        return defaultValue;
    }

    /// <summary>
    /// Key: REVSHARP_DEBUG_WSLOG
    /// Default: false
    /// 
    /// Print to console all websocket messages sent/received.
    /// </summary>
    internal static bool WebsocketDebugLogging => ParseBool("REVSHARP_DEBUG_WSLOG", false);
    
    /// <summary>
    /// Key: REVSHARP_LOG_COLOR
    /// Default: true
    /// 
    /// Change console text/background color on logging.
    /// </summary>
    internal static bool EnableLogColor => ParseBool("REVSHARP_LOG_COLOR", true);

    /// <summary>
    /// Key: REVSHARP_LOGFLAG
    /// Default: 30
    /// 
    /// What log level to use. Any log level with a value equal or lower than this will be printed.
    ///
    /// Note:
    /// The value will be parsed as an integer then type casted to <see cref="LogFlag"/>.
    /// </summary>
    internal static LogFlag LogFlags
    {
        get
        {
            var item = (LogFlag)ParseInt("REVSHARP_LOGFLAG", (int)LogFlag.Information);
            return item;
        }
    }
}
