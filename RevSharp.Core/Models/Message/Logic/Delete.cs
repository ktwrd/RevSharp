using System.Net;

namespace RevSharp.Core.Models;

public partial class Message
{
    
    public Task<bool> Delete(Client client)
    {
        return Delete(client, ChannelId, Id);
    }
    public Task<bool> Delete()
        => Delete(Client);
    internal static async Task<bool> Delete(Client client, string channelId, string messageId)
    {
        var response = await client.DeleteAsync($"/channels/{channelId}/messages/{messageId}");
        return response.StatusCode == HttpStatusCode.NoContent;
    }
}