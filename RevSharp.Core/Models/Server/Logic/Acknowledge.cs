namespace RevSharp.Core.Models;

public partial class Server
{
    /// <summary>
    /// Mark server as read
    /// </summary>
    /// <returns>Was it successful</returns>
    public async Task<bool> Acknowledge(Client client)
    {
        var response = await client.PutAsync($"/servers/{Id}/ack");
        return (int)response.StatusCode == 204;
    }

    /// <summary>
    /// Mark server as read
    /// </summary>
    /// <returns>Was it successful</returns>
    public Task<bool> Acknowledge()
        => Acknowledge(Client);
}