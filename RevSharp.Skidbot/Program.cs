using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using RevSharp.Core;
using RevSharp.Core.Models;
using RevSharp.Skidbot.Models;
using RevSharp.Skidbot.Reflection;
using File = System.IO.File;

namespace RevSharp.Skidbot;

public static class Program
{
    public static void Main(string[] args)
    {
        StartTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        AsyncMain(args).Wait();
    }

    public static async Task AsyncMain(string[] args)
    {
        ReadConfig();
        Client = new Client(ConfigData.Token, ConfigData.IsBot);
        Client.Ready += () =>
        {
            var c = Client.GetChannel(ConfigData.PublicLogChannelId).Result;
            var plugins = Reflection.GetPlugins();
            c.SendMessage(
                new SendableEmbed()
                {
                    Title = $"Running Skidbot v{Version}",
                    Description = string.Join(
                        "\n", new string[]
                        {
                            $"Loaded {plugins.Length} plugins;",
                            string.Join("\n",  plugins.Select(v => $"- `{v}`"))
                        })
                }).Wait();
        };
        await InitializeModules();
        await Client.LoginAsync();
        await Task.Delay(-1);
    }
    private static ReflectionInclude Reflection { get; set; }

    private static async Task InitializeModules()
    {
        var i = new ReflectionInclude(Client)
        {
            Config = ConfigData
        };
        LoadLocalAssemblies();
        var allAsm = AppDomain.CurrentDomain.GetAssemblies();
        var ownAssembly = typeof(Program).Assembly;
        var sdkAssembly = typeof(ReflectionInclude).Assembly;
        await i.Search(typeof(Program).Assembly);
        Log.WriteLine("Searching in other Skidbot assemblies");

        foreach (var a in allAsm)
        {
            if (a.FullName.Contains("RevSharp.Skidbot") && a.FullName != ownAssembly.FullName && a.FullName != sdkAssembly.FullName)
            {
                Log.WriteLine($"Searching Assembly {a.FullName}");
                await i.Search(a);
            }
        }

        await i.SearchFinale();
        Reflection = i;
    }

    private static void LoadLocalAssemblies()
    {
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        var loadedPaths = loadedAssemblies.Select(a => a.Location).ToArray();
            
        var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
        var toLoad = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();

        toLoad.ForEach(path => loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path))));
    }
    public static void Quit(int exitCode = 0)
    {
        BeforeQuit();
        Environment.Exit(exitCode);
    }
    private static void BeforeQuit()
    {
        WriteConfig();
        Client.DisconnectAsync().Wait();
    }
    public static long StartTimestamp { get; private set; }
    public static string GetUptimeString()
    {
        var current = DateTimeOffset.UtcNow;
        var start = DateTimeOffset.FromUnixTimeSeconds(StartTimestamp);
        var diff = current - start;
        var data = new List<string>();
        if (Math.Floor(diff.TotalHours) > 0)
            data.Add($"{Math.Floor(diff.TotalHours)}hr");
        if (diff.Minutes > 0)
            data.Add($"{diff.Minutes}m");
        if (diff.Seconds > 0)
            data.Add($"{diff.Seconds}s");
        return string.Join(" ", data);
    }
    #region Fields
    public static RevSharp.Core.Client Client;
    public static Random Random => new Random();
    public static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
    {
        IgnoreReadOnlyFields = true,
        IgnoreReadOnlyProperties = true,
        IncludeFields = true,
        WriteIndented = true
    };

    public static string Version
    {
        get
        {
            string result = "";
            var targetAppend = VersionRaw;
            result += targetAppend ?? "null_version";
#if DEBUG
            result += "-DEBUG";
#endif
            return result;
        }
    }

    public static string VersionFull => $"{Version} ({VersionDate})";

    public static DateTime VersionDate
    {
        get
        {
            DateTime buildDate = new DateTime(2000, 1, 1)
                .AddDays(VersionReallyRaw?.Build ?? 0)
                .AddSeconds((VersionReallyRaw?.Revision ?? 0) * 2);
            return buildDate;
        }
    }

    private static string? VersionRaw
    {
        get
        {
            return VersionReallyRaw?.ToString() ?? null;
        }
    }

    internal static Version? VersionReallyRaw
    {
        get
        {
            var asm = Assembly.GetAssembly(typeof(Program));
            var name = asm?.GetName();
            if (name == null || name.Version == null)
            {
                if (name == null)
                {
                    Log.Warn($"Assembly.GetName() resulted in null (when Assembly is from {asm?.Location})");
                }
                else if (name.Version == null)
                {
                    Log.Warn($"Assembly.GetName().Version is null (when Assembly is from {asm?.Location})");
                }
                return null;
            }
            return name.Version;
        }
    }
    #endregion
    #region Config
    public static ConfigData ConfigData { get; set; }
    public static string ConfigLocation => FeatureFlags.ConfigLocation;

    public static void ReadConfig()
    {
        if (!File.Exists(ConfigLocation))
            WriteConfig();
        var content = File.ReadAllText(ConfigLocation);
        var deser = JsonSerializer.Deserialize<ConfigData>(content, SerializerOptions);
        ConfigData = deser;
    }

    public static void WriteConfig()
    {
        var parentDir = Path.GetDirectoryName(ConfigLocation);
        if (!Directory.Exists(parentDir))
            Directory.CreateDirectory(parentDir);
        var ser = JsonSerializer.Serialize(ConfigData, SerializerOptions);
        File.WriteAllText(ConfigLocation, ser);
    }
    #endregion
}


