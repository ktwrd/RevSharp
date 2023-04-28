using System.Text.Json.Serialization;

namespace RevSharp.Core.Models;


/// <summary>
/// Personal "Saved Notes" channel which allows users to save messages
/// </summary>
public class SavedMessagesChannel : BaseChannel, IFetchable
{
    /// <summary>
    /// Id of the user this channel belongs to
    /// </summary>
    [JsonPropertyName("user")]
    public string UserId { get; set; }

    public async Task<bool> Fetch(Client client)
    {
        var data = await GetGeneric<SavedMessagesChannel>(client);
        if (data == null)
            return false;
        UserId = data.UserId;
        return true;
    }
}