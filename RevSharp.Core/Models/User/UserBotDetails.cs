using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class UserBotDetails /*: IUserBotDetails*/
{
    [JsonPropertyName("owner")]
    public string OwnerId { get; set; }
    [JsonIgnore]
    public User? Owner { get; internal set; }
}
