using MongoDB.Driver;
using RevSharp.Skidbot.Models;
using RevSharp.Skidbot.Reflection;

namespace RevSharp.Skidbot.Modules;

public class BaseMongoController<TH> : BaseModule where TH : BaseMongoModel
{
    private string ConnectionString { get; set; }
    public IMongoDatabase Database { get; private set; }
    private MongoClient _client { get; set; }
    protected string CollectionName { get; private set; }

    public BaseMongoController(string collectionName)
        : base()
    {
        CollectionName = collectionName;
    }
    public override async Task Initialize(ReflectionInclude reflection)
    {
        ConnectionString = Program.ConfigData.MongoConnectionUrl;
        await DatabaseInit();
    }

    public async Task DatabaseInit()
    {
        try
        {
            _client = new MongoClient(ConnectionString);
            await _client.StartSessionAsync();
            Database = _client.GetDatabase(Program.ConfigData.MongoDatabaseName);
        }
        catch (Exception e)
        {
            Log.Error("Failed to initialize MongoDB connection");
            Log.Error(e);
            Environment.Exit(1);
            throw;
        }
    }
    
    protected IMongoCollection<T>? GetCollection<T>(string name)
        => Database.GetCollection<T>(name);

    protected IMongoCollection<T>? GetCollection<T>()
        => Database.GetCollection<T>(CollectionName);

    protected IMongoCollection<TH>? GetCollection()
        => GetCollection<TH>();

    protected IMongoCollection<TH>? GetCollection(string name)
        => GetCollection<TH>(name);
    
}