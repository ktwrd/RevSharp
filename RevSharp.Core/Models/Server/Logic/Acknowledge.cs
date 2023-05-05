namespace RevSharp.Core.Models;

public partial class Server
{
    public async Task<bool> Acknowledge(Client client)
    {
        var response = await client.PutAsync($"/servers/{Id}/ack");
        return (int)response.StatusCode == 204;
    }

    public Task<bool> Acknowledge()
        => Acknowledge(Client);
}