namespace RevSharp.Xenia.Models;

public class StarboardConfigModel : BaseMongoModel
{
    public string ServerId { get; set; }
    public int MinimumRequired { get; set; }
    public string ChannelId { get; set; }
    /// <summary>
    /// Key: Original Message Id
    /// Value: Message Id in ChannelId
    /// </summary>
    public Dictionary<string, string> ProxyMessageMap { get; set; }
    public Dictionary<string, int> MessageReact { get; set; }
    public StarboardConfigModel()
    {
        ServerId = "";
        MinimumRequired = 3;
        ChannelId = "";
        ProxyMessageMap = new Dictionary<string, string>();
        MessageReact = new Dictionary<string, int>();
    }
}