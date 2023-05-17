using kate.shared.Helpers;
using RevSharp.Core.Helpers;
using RevSharp.Core.Models;

namespace RevSharp.Core;

public partial class Client
{
    public event ChannelDelegate ChannelCreated;

    internal void OnChannelCreated(BaseChannel channel)
    {
        if (ChannelCreated != null)
            ChannelCreated?.Invoke(channel);
    }
    
    public event MessageDelegate MessageReceived;
    public event VoidDelegate Ready;
    public event VoidDelegate ClientAuthenticated;
    public event StringDelegate ErrorReceived;
    public event MessageDeleteDelegate MessageDeleted;

    public event ServerDelegate ServerCreate;

    internal void OnServerCreate(Server server)
    {
        AddToCache(server);
        if (ServerCreate != null)
        {
            ServerCreate?.Invoke(server);
        }
    }
    internal void OnMessageDeleted(string messageId, string channelId)
    {
        if (MessageDeleted != null)
        {
            MessageDeleted?.Invoke(messageId, channelId);
        }
    }
}