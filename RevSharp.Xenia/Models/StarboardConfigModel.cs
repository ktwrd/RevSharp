namespace RevSharp.Xenia.Models;

public class StarboardConfigModel : BaseMongoModel
{
    public string ServerId { get; set; }
    public int MinimumRequired { get; set; }
    public string ChannelId { get; set; }

    public StarboardConfigModel()
    {
        ServerId = "";
        MinimumRequired = 3;
        ChannelId = "";
    }
}