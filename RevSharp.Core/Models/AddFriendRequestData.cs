using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;

public class AddFriendRequestData
{
    /// <summary>
    /// Username and discriminator combo separated by #
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; }
}