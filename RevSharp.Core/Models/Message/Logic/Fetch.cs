namespace RevSharp.Core.Models;

public partial class Message
{
    public async Task<Server?> FetchServer(Client client)
    {
        var channel = await client.GetChannel(ChannelId, false);
        if (channel == null)
            return null;
        if (!(channel is TextChannel textChannel))
            return null;
        var server = await client.GetServer(textChannel.ServerId, false);
        return server;
    }

    public Task<Server?> FetchServer()
        => FetchServer(Client);
}