namespace RevSharp.Xenia.Models;

public class LevelUserModel : BaseMongoModel
{
    public string UserId { get; set; }
    /// <summary>
    /// Key: ServerID
    /// Value: XP
    /// </summary>
    public Dictionary<string, ulong> ServerPair { get; set; }
    public long LastMessageTimestamp { get; set; }
    public string LastMessageId { get; set; }
    public string LastMessageChannelId { get; set; }

    public LevelUserModel()
    {
        UserId = "";
        ServerPair = new();
        LastMessageTimestamp = 0;
        LastMessageId = "";
        LastMessageChannelId = "";
    }
}