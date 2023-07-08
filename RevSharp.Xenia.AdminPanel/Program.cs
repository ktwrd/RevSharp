using System.Text.Json;
using MongoDB.Driver;
using RevSharp.Core;
using RevSharp.Xenia;

public static class Program
{
    public static Client RevoltClient { get; set; }
    public static void Main(string[] args)
    {
        LoadConfig();
        DatabaseConnect().Wait();
        RevoltClient = new Client(Config.Token, Config.IsBot);
        RevoltClient.LoginAsync();
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
    public static void LoadConfig()
    {
        var location = FeatureFlags.ConfigLocation;
        if (!File.Exists(location))
        {
            Log.Error("Config not found");
            Environment.Exit(1);
        }

        var cfg = JsonSerializer.Deserialize<ConfigData>(File.ReadAllText(location), SerializerOptions);
        if (cfg == null)
        {
            Log.Error("Failed to deserialize config (is null)");
            Environment.Exit(1);
        }

        Config = cfg;
    }
    public static ConfigData Config { get; set; }
    public static async Task DatabaseConnect()
    {
        Log.WriteLine("Connecting to MongoDB Database");
        try
        {
            var connectionSettings = MongoClientSettings.FromConnectionString(Config.MongoConnectionUrl);
            connectionSettings.VerifySslCertificate = false;
            MongoClient = new MongoClient(connectionSettings);
            await MongoClient.StartSessionAsync();
            Database = MongoClient.GetDatabase(Config.MongoDatabaseName);
        }
        catch (Exception ex)
        {
            Log.Error("Failed to initialize MongoDB");
            Log.Error(ex);
            Environment.Exit(1);
            throw;
        }
    }
    public static IMongoDatabase Database { get; set; }
    public static MongoClient MongoClient { get; set; }

    public static JsonSerializerOptions SerializerOptions => Client.SerializerOptions;
}