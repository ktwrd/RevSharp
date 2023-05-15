using MongoDB.Driver;
using RevSharp.ReBot.Models;
using RevSharp.ReBot.Reflection;

namespace RevSharp.ReBot.Modules;

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
        ConnectionString = Program.Config.MongoConnectionUrl;
        await DatabaseInit();
    }

    public async Task DatabaseInit()
    {
        try
        {
            _client = new MongoClient(ConnectionString);
            Console.WriteLine($"Creating MongoDB Session");
            await _client.StartSessionAsync();
            Console.WriteLine($"Connected to MongoDB Server");
            Database = _client.GetDatabase(Program.Config.MongoDatabaseName);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Failed to initialize MongoDB connection");
            Console.Error.WriteLine(e);
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