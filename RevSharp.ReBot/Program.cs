using System.Diagnostics;
using System.Text.Json;
using RevSharp.Core;
using RevSharp.ReBot.Reflection;

namespace RevSharp.ReBot;

public static class Program
{
    public static RevSharp.Core.Client Client;
    public static void Main(string[] args)
    {
        AsyncMain(args).Wait();
    }

    public static async Task AsyncMain(string[] args)
    {
        ReadConfig();
        Client = new Client(Config.Token, Config.IsBot);
        await InitializeModules();
        await Client.LoginAsync();
        Client.MessageReceived += (message) =>
        {
            Console.WriteLine(string.Join("\n", new string[]
            {
                $"Content: {message.Content}",
                $"Author:  {message.AuthorId}",
                $"Channel: {message.ChannelId}"
            }));
        };
        await Task.Delay(-1);
    }

    private static async Task InitializeModules()
    {
        var i = new ReflectionInclude(Client);
        await i.Search(typeof(Program).Assembly);
    }
    
    public static Config Config { get; set; }

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

    public static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
    {
        IgnoreReadOnlyFields = true,
        IgnoreReadOnlyProperties = true,
        IncludeFields = true
    };
    public static void ReadConfig()
    {
        if (!File.Exists(ConfigLocation))
            WriteConfig();
        var content = File.ReadAllText(ConfigLocation);
        var deser = JsonSerializer.Deserialize<Config>(content, SerializerOptions);
        Config = deser;
    }

    public static void WriteConfig()
    {
        var parentDir = Path.GetDirectoryName(ConfigLocation);
        if (!Directory.Exists(parentDir))
            Directory.CreateDirectory(parentDir);
        var ser = JsonSerializer.Serialize(Config, SerializerOptions);
        File.WriteAllText(ConfigLocation, ser);
    }
}

public class Config
{
    public string Token { get; set; }
    public bool IsBot { get; set; }
}