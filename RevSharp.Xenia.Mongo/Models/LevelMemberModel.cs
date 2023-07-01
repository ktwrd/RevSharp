namespace RevSharp.Xenia.Models;

public class LevelMemberModel : BaseMongoModel
{
    public string UserId { get; set; }
    public string ServerId { get; set; }
    public ulong Xp { get; set; }
    public long LastMessageTimestamp { get; set; }
    public string LastMessageId { get; set; }
    public string LastMessageChannelId { get; set; }
    public LevelMemberModel()
    {
        UserId = "";
        ServerId = "";
        Xp = 0;
        LastMessageTimestamp = 0;
        LastMessageId = "";
        LastMessageChannelId = "";
    }
}