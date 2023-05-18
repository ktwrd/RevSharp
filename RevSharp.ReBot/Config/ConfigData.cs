namespace RevSharp.ReBot;

public class ConfigData
{
    public string Token { get; set; }
    public bool IsBot { get; set; }
    public string Prefix { get; set; }
    public string MongoConnectionUrl { get; set; }
    public string MongoDatabaseName { get; set; }
    public GCSConfig GoogleCloud { get; set; }

    public ConfigData()
    {
        Token = "";
        IsBot = false;
        Prefix = "r.";
        MongoConnectionUrl = "mongodb://user:password@localhost:27021";
        MongoDatabaseName = "skidbot_revolt";
        GoogleCloud = new GCSConfig();
    }
}