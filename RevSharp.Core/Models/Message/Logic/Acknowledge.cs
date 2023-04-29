using System.Net;
using System.Text.Json;
using kate.shared.Helpers;

namespace RevSharp.Core.Models;

public partial class Message
{
    public async Task<bool> Acknowledge(Client client)
    {
        var response = await client.PutAsync($"/channels/{ChannelId}/ack/{Id}");
        return response.StatusCode == HttpStatusCode.NoContent;
    }
    public Task<bool> Acknowledge()
        => Acknowledge(Client);
    
    
}