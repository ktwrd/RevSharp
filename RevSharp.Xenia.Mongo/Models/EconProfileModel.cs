
namespace RevSharp.Xenia.Models;

public class EconProfileModel : BaseMongoModel
{
    public string UserId { get; set; }
    public string ServerId { get; set; }
    public long Coins { get; set; }
    public long LastDailyTimestamp { get; set; }
}