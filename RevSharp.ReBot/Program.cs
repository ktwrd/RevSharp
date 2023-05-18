using System.Diagnostics;
using System.Text.Json;
using RevSharp.Core;
using RevSharp.ReBot.Models;
using RevSharp.ReBot.Reflection;

namespace RevSharp.ReBot;

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
    #endregion
    #region Config
    public static ConfigData ConfigData { get; set; }
    public static string ConfigLocation
    {
        get
        {
            var env = Environment.GetEnvironmentVariable("REBOT_CONFIG_LOCATION");
            if (env != null)
                return env;
            
            return Path.Combine(
                Directory.GetCurrentDirectory(),
                "config.json");
        }
    }

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


