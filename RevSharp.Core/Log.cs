using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RevSharp.Core;

internal static class Log
{
    internal struct LogColor
        {
            internal ConsoleColor Foreground;
            internal ConsoleColor Background;
        }
        #region Init Colors
        private static LogColor WarnColor = new LogColor
        {
            Background = ConsoleColor.DarkYellow,
            Foreground = ConsoleColor.White
        };
        private static LogColor ErrorColor = new LogColor
        {
            Background = ConsoleColor.DarkRed,
            Foreground = ConsoleColor.White
        };
        private static LogColor DebugColor = new LogColor
        {
            Background = ConsoleColor.DarkBlue,
            Foreground = ConsoleColor.White
        };

        private static LogColor NoteColor = new LogColor
        {
            Background = ConsoleColor.Magenta,
            Foreground = ConsoleColor.Black
        };

        private static LogColor VerboseColor = new LogColor()
        {
            Background = ConsoleColor.DarkGreen,
            Foreground = ConsoleColor.White
        };
        private static LogColor DefaultColor = new LogColor
        {
            Background = ConsoleColor.Black,
            Foreground = ConsoleColor.White
        };
        #endregion

        /*private static List<string> linequeue = new List<string>();
        private static System.Timers.Timer _timer = null;
        internal static string LogOutput => Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"log_{Program.StartTimestamp}.txt");
        private static void CreateTimer()
        {
            if (_timer != null) return;
            if ((Environment.GetEnvironmentVariable("SKIDBOT_WRITELOG") ?? "true") == "false")
                return;
            if (!Directory.Exists(Path.GetDirectoryName(LogOutput)))
                Directory.CreateDirectory(Path.GetDirectoryName(LogOutput));
            _timer = new System.Timers.Timer();
            _timer.Interval = 5000;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Enabled = true;
            _timer.Start();
        }

        private static void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timer.Stop();
            string[] lines = linequeue.ToArray();
            linequeue.Clear();
            File.AppendAllLines(LogOutput, lines);
            _timer.Start();
        }
        */

        internal static void SetColor(LogColor? color = null)
        {
            if (!FeatureFlags.EnableLogColor)
                return;
            LogColor targetColor = color ?? DefaultColor;
            Console.BackgroundColor = targetColor.Background;
            Console.ForegroundColor = targetColor.Foreground;
        }

        private static string CriticalPrefix = "[CRIT]";
        private static string WarnPrefix = "[WARN]";
        private static string ErrorPrefix = "[ERR] ";
        private static string LogPrefix = "[LOG] ";
        private static string DebugPrefix = "[DEBG]";
        private static string NotePrefix = "[NOTE]";
        private static string VerbosePrefix = "[VERB]";
        private static bool ShowMethodName = true;
        private static bool ShowTimestamp = false;

        private static Dictionary<LogFlag, (LogColor, string)> FlagConfig = new Dictionary<LogFlag, (LogColor, string)>()
        {
            {LogFlag.Critical, (ErrorColor, CriticalPrefix)},
            {LogFlag.Error, (ErrorColor, ErrorPrefix)},
            {LogFlag.Warning, (WarnColor, WarnPrefix)},
            {LogFlag.Information, (DefaultColor, LogPrefix)},
            {LogFlag.Note, (NoteColor, NotePrefix)},
            {LogFlag.Verbose, (VerboseColor, VerbosePrefix)},
            {LogFlag.Debug, (DebugColor, DebugPrefix)}
        };
        internal static void Critical(string content, [CallerMemberName] string methodname = null,
            [CallerFilePath] string methodfile = null)
            => WriteLine(content, LogFlag.Critical, ShowMethodName, methodname, methodfile);
        internal static void Error(string content, [CallerMemberName] string methodname = null,
            [CallerFilePath] string methodfile = null)
            => WriteLine(content, LogFlag.Error, ShowMethodName, methodname, methodfile);
        internal static void Warn(string content, [CallerMemberName] string methodname = null,
            [CallerFilePath] string methodfile = null)
            => WriteLine(content, LogFlag.Warning, ShowMethodName, methodname, methodfile);
        internal static void Info(string content, [CallerMemberName] string methodname = null,
            [CallerFilePath] string methodfile = null)
            => WriteLine(content, LogFlag.Information, ShowMethodName, methodname, methodfile);
        internal static void Note(string content, [CallerMemberName] string methodname = null,
            [CallerFilePath] string methodfile = null)
            => WriteLine(content, LogFlag.Note, ShowMethodName, methodname, methodfile);
        internal static void Verbose(string content, [CallerMemberName] string methodname = null,
            [CallerFilePath] string methodfile = null)
            => WriteLine(content, LogFlag.Verbose, ShowMethodName, methodname, methodfile);
        internal static void Debug(string content, [CallerMemberName] string methodname = null,
            [CallerFilePath] string methodfile = null)
            => WriteLine(content, LogFlag.Debug, ShowMethodName, methodname, methodfile);
        #region Object Overload
        internal static void Critical(object content, [CallerMemberName] string methodname = null,
            [CallerFilePath] string methodfile = null)
            => WriteLine(content.ToString(), LogFlag.Critical, ShowMethodName, methodname, methodfile);
        internal static void Error(object content, [CallerMemberName] string methodname = null,
            [CallerFilePath] string methodfile = null)
            => WriteLine(content.ToString(), LogFlag.Error, ShowMethodName, methodname, methodfile);
        internal static void Warn(object content, [CallerMemberName] string methodname = null,
            [CallerFilePath] string methodfile = null)
            => WriteLine(content.ToString(), LogFlag.Warning, ShowMethodName, methodname, methodfile);
        internal static void Info(object content, [CallerMemberName] string methodname = null,
            [CallerFilePath] string methodfile = null)
            => WriteLine(content.ToString(), LogFlag.Information, ShowMethodName, methodname, methodfile);
        internal static void Note(object content, [CallerMemberName] string methodname = null,
            [CallerFilePath] string methodfile = null)
            => WriteLine(content.ToString(), LogFlag.Note, ShowMethodName, methodname, methodfile);
        internal static void Verbose(object content, [CallerMemberName] string methodname = null,
            [CallerFilePath] string methodfile = null)
            => WriteLine(content.ToString(), LogFlag.Verbose, ShowMethodName, methodname, methodfile);
        internal static void Debug(object content, [CallerMemberName] string methodname = null,
            [CallerFilePath] string methodfile = null)
            => WriteLine(content.ToString(), LogFlag.Debug, ShowMethodName, methodname, methodfile);
        #endregion

        internal static void WriteLine(
            string content,
            LogColor? color = null,
            string prefix = null,
            bool fetchMethodName = true,
            [CallerMemberName] string methodname = null,
            [CallerFilePath] string methodfile = null)
        {
            // CreateTimer();
            SetColor(color);
            if (methodname != null && fetchMethodName && methodfile != null)
                content = $"{FormatMethodName(methodname, methodfile)}{content}";
            string pfx = (prefix ?? LogPrefix) + " ";
            Console.WriteLine(pfx + content);
            // linequeue.Add(pfx + content);
            SetColor();
        }

        internal static void WriteLine(string content, LogFlag flag, bool fetchMethodName = true,
            [CallerMemberName] string methodname = null, [CallerFilePath] string methodfile = null)
        {
            // When flag isn't as important as LogFlag we ignore
            if ((int)flag > (int)FeatureFlags.LogFlags)
                return;
            (LogColor color, string prefix) = FlagConfig[flag];
            WriteLine(
                content,
                color,
                prefix,
                fetchMethodName,
                methodname,
                methodfile);
        }
        private static string FormatMethodName(string methodName, string methodFilePath)
        {
            var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (methodName != null)
            {
                if (methodFilePath != null)
                    if (ShowTimestamp)
                        return $"[{Path.GetFileNameWithoutExtension(methodFilePath)}->{methodName}:{ts}] ";
                    else
                        return $"[{Path.GetFileNameWithoutExtension(methodFilePath)}->{methodName}] ";
                if (ShowTimestamp)
                    return $"[unknown->{methodName}:{ts}] ";
                return $"[unknown->{methodName}] ";
            }
            else if (methodFilePath != null)
                if (ShowTimestamp)
                    return $"[{Path.GetFileNameWithoutExtension(methodFilePath)}:{ts}] ";
                else
                    return $"[{Path.GetFileNameWithoutExtension(methodFilePath)}] ";
            if (ShowTimestamp)
                return $"[{ts}] ";
            return "";
        }
}