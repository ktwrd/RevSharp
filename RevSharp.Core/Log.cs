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
        internal static LogColor WarnColor = new LogColor
        {
            Background = ConsoleColor.DarkYellow,
            Foreground = ConsoleColor.White
        };
        internal static LogColor ErrorColor = new LogColor
        {
            Background = ConsoleColor.DarkRed,
            Foreground = ConsoleColor.White
        };
        internal static LogColor DebugColor = new LogColor
        {
            Background = ConsoleColor.DarkBlue,
            Foreground = ConsoleColor.White
        };

        internal static LogColor NoteColor = new LogColor
        {
            Background = ConsoleColor.Magenta,
            Foreground = ConsoleColor.Black
        };
        internal static LogColor DefaultColor = new LogColor
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
            
            string? envEnableColor = Environment.GetEnvironmentVariable("REVSHARP_LOG_COLOR");
            if ((envEnableColor ?? "true") != "true")
                return;
            LogColor targetColor = color ?? DefaultColor;
            Console.BackgroundColor = targetColor.Background;
            Console.ForegroundColor = targetColor.Foreground;
        }
        internal static string WarnPrefix = "[WARN]";
        internal static string ErrorPrefix = "[ERR] ";
        internal static string LogPrefix = "[LOG] ";
        internal static string DebugPrefix = "[DEBG]";
        internal static string NotePrefix = "[NOTE]";
        internal static bool ShowMethodName = true;
        internal static bool ShowTimestamp = false;

        internal static void Warn(string content, [CallerMemberName] string methodname = null,
            [CallerFilePath] string methodfile = null)
            => WriteLine(content, WarnColor, WarnPrefix, ShowMethodName, methodname, methodfile);

        internal static void Error(string content, [CallerMemberName] string methodname = null,
            [CallerFilePath] string methodfile = null)
            => WriteLine(content, ErrorColor, ErrorPrefix, ShowMethodName, methodname, methodfile);

        internal static void Debug(string content, [CallerMemberName] string methodname = null,
            [CallerFilePath] string methodfile = null)
            => WriteLine(content, DebugColor, DebugPrefix, ShowMethodName, methodname, methodfile);

        internal static void Note(string content, [CallerMemberName] string methodname = null,
            [CallerFilePath] string methodfile = null)
            => WriteLine(content, NoteColor, NotePrefix, ShowMethodName, methodfile, methodfile);

        #region Object Overload
        internal static void Warn(object content, [CallerMemberName] string methodname = null, [CallerFilePath] string methodfile = null)
            => Warn(content.ToString(), methodname, methodfile);
        internal static void Error(object content, [CallerMemberName] string methodname = null, [CallerFilePath] string methodfile = null)
            => Error(content.ToString(), methodname, methodfile);
        internal static void Debug(object content, [CallerMemberName] string methodname = null, [CallerFilePath] string methodfile = null)
            => Debug(content.ToString(), methodname, methodfile);
        internal static void WriteLine(object content, [CallerMemberName] string methodname = null, [CallerFilePath] string methodfile = null)
            => WriteLine(content.ToString(), methodname, methodfile);
        #endregion

        internal static void WriteLine(string content, LogColor? color = null, string prefix = null, bool fetchMethodName = true, [CallerMemberName] string methodname = null, [CallerFilePath] string methodfile = null)
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