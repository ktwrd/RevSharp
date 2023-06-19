﻿using System.Diagnostics;
using System.Text.Json;
using RevSharp.Core;
using RevSharp.Skidbot.Models;
using RevSharp.Skidbot.Reflection;

namespace RevSharp.Skidbot;

public static class Program
{
    public static void Main(string[] args)
    {
        AsyncMain(args).Wait();
    }

    public static async Task AsyncMain(string[] args)
    {
        ReadConfig();
        Client = new Client(ConfigData.Token, ConfigData.IsBot);
        await InitializeModules();
        await Client.LoginAsync();
        await Task.Delay(-1);
    }

    private static async Task InitializeModules()
    {
        var i = new ReflectionInclude(Client);
        await i.Search(typeof(Program).Assembly);
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


