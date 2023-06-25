namespace RevSharp.Core.Models;

public partial class Message
{
    public async Task<Server?> FetchServer(Client client, bool forceUpdate = false)
    {
        var channel = await client.GetChannel(ChannelId, forceUpdate: forceUpdate);
        if (channel == null)
            return null;
        if (!(channel is TextChannel textChannel))
            return null;
        var server = await client.GetServer(textChannel.ServerId, forceUpdate: forceUpdate);
        return server;
    }

    public Task<Server?> FetchServer(bool forceUpdate = false)
        => FetchServer(Client, forceUpdate);
}