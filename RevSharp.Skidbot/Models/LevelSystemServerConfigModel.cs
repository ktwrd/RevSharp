namespace RevSharp.Skidbot.Models;

public class LevelSystemServerConfigModel : BaseMongoModel
{
    public string ServerId { get; set; }
    public string? LogChannelId { get; set; }
    public bool Enable { get; set; }

    public LevelSystemServerConfigModel()
    {
        ServerId = "";
        LogChannelId = null;
        Enable = true;
    }
}